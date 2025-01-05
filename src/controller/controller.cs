using System.Diagnostics;
using LightAssistant.Interfaces;
using LightAssistant.Utils;

namespace LightAssistant.Controller;

internal partial class Controller : IController
{
    private readonly IConsoleOutput _consoleOutput;
    private readonly List<IDeviceBus> _deviceBuses;
    private readonly IUserInterface _guiApp;
    private readonly DeviceServiceMapping _deviceServiceMapping;
    private readonly string _dataPath;
    private readonly int _openNetworkTimeSeconds;
    private readonly Thread _scheduleThread;

    // Protected data
    private readonly SlimReadWriteDataGuard<DeviceInfoCollection> _devices = new([]);
    private readonly SlimReadWriteDataGuard<DeviceSchedule> _schedules = new([]);
    private readonly SlimReadWriteDataGuard<DeviceRoutes> _routes = new([]);
    private readonly SlimReadWriteDataGuard<ServiceOptionValues> _serviceOptionValues = new([]);

    public Controller(IConsoleOutput consoleOutput, List<IDeviceBus> deviceBuses, IUserInterface guiApp, string dataPath, int openNetworkTimeSeconds)
    {
        _consoleOutput = consoleOutput;
        _deviceBuses = deviceBuses;
        _guiApp = guiApp;
        _deviceServiceMapping = new DeviceServiceMapping(_consoleOutput);
        _dataPath = dataPath;
        _openNetworkTimeSeconds = openNetworkTimeSeconds;

        foreach(var bus in _deviceBuses) {
            bus.DeviceDiscovered += HandleDeviceDiscovered;
            bus.DeviceUpdated += HandleDeviceUpdated;
            bus.DeviceAction += HandleDeviceAction;
            bus.NetworkOpenStatus += _guiApp.NetworkOpenStatusChanged; // TODO: This should be specific for each client. Also reflected in the UI.
        }
        _guiApp.AppController = this;

        LoadData();
        _scheduleThread = new Thread(ScheduleRunner);
        _scheduleThread.Start();
    }

    private async Task SaveData()
    {
        if(string.IsNullOrWhiteSpace(_dataPath))
            return; // Error written in LoadData

        try {
            var data = new RunTimeData();

            using(_ = _serviceOptionValues.ObtainWriteLock(out var serviceOptionValues))
                data.SetServiceOptionValues(serviceOptionValues);
            using(_ = _routes.ObtainReadLock(out var routes))
                data.SetRoutes(routes);
            using(_ = _schedules.ObtainReadLock(out var schedules))
                data.SetSchedules(schedules);

            await data.SaveToFile(_dataPath);
        }
        catch(Exception ex) {
            _consoleOutput.ErrorLine("Could not save runtime data. Message: " + ex.Message);
        }
    }

    private void LoadData()
    {
        if(string.IsNullOrWhiteSpace(_dataPath)) {
            _consoleOutput.ErrorLine("DataPath (specified in config file) was empty or whitespace. This does not work.");
            _consoleOutput.ErrorLine("WARNING: Configuration data will not be saved!!!");
            return;
        }

        RunTimeData? data;
        string errMsg = "Unknown reason.";
        try {
            data = RunTimeData.LoadFromFile(_dataPath);
        }
        catch(Exception ex) {
            errMsg = ex.Message;
            data = null;
        }

        if(data == null) {
            _consoleOutput.ErrorLine($"Could not configuration data from file '{_dataPath}'. Message:" + errMsg);
            return;
        }

        using(_ = _routes.ObtainWriteLock(out var routes))
            data.PopulateRoutes(routes);

        using(_ = _schedules.ObtainWriteLock(out var schedules))
            data.PopulateSchedules(schedules);

        using(_ = _serviceOptionValues.ObtainWriteLock(out var serviceOptionValues))
            data.PopulateServiceOptionValues(serviceOptionValues);
    }

    public IReadOnlyList<IDevice> GetDeviceList()
    {
        using var _ = _devices.ObtainReadLock(out var devices);
        return new List<IDevice>(devices.Keys);
    }

    public IDevice? TryGetDevice(string address)
    {
        using var _ = _devices.ObtainReadLock(out var devices);
        return devices.Keys.FirstOrDefault(entry => entry.Address == address);
    }

    public bool TryGetDeviceStatus(IDevice device, out Dictionary<string, string>? status)
    {
        using var _ = _devices.ObtainReadLock(out var devices);

        if(devices.TryGetValue(device, out var entry)) {
            status = new Dictionary<string, string>(entry.Status);
            return true;
        }

        status = null;
        return false;
    }

    public IRoutingOptions? GetRoutingOptionsFor(IDevice device)
    {
        using var _ = _devices.ObtainReadLock(out var devices);

        if(!devices.TryGetValue(device, out var deviceInfo)) {
            _consoleOutput.ErrorLine($"Device not found. Name: {device.Name}");
            return null;
        }

        static string GetInternalEventName(Type ev) => ev.Name.Replace("InternalEvent_", "");

        var providedEvents = deviceInfo.Services.ProvidedEvents
            .Select(ev => new ProvidedEvent(GetInternalEventName(ev.EventType), ev.Name))
            .ToList();
        var consumedEvents = deviceInfo.Services.ConsumedEvents
            .Select(ev => new ConsumableEvent(GetInternalEventName(ev.EventType), ev.TargetName))
            .ToList();
        return new RoutingOptions(providedEvents, consumedEvents);
    }

    public IReadOnlyList<IConsumableAction> GetConsumableActionsFor(IDevice device)
    {
        using var _ = _devices.ObtainReadLock(out var devices);

        if(!devices.TryGetValue(device, out var deviceInfo)) {
            _consoleOutput.ErrorLine($"Device not found. Name: {device.Name}");
            return [];
        }
        return deviceInfo.Services.ConsumedActions
            .Select(ev => new ConsumableAction(ev.Name, ev.Params))
            .ToList();
    }

    public IReadOnlyList<IServiceOption> GetServiceOptionsFor(IDevice device)
    {
        using var _ = _devices.ObtainReadLock(out var devices);

        if(!devices.TryGetValue(device, out var deviceInfo)) {
            _consoleOutput.ErrorLine($"Device not found. Name: {device.Name}");
            return [];
        }
        return deviceInfo.Services.ServiceOptions.ToList();
    }

    private void HandleDeviceAction(IDevice device, Dictionary<string, string> data)
    {
        bool statusUpdated;
        Dictionary<string, string> newStatus;
        DeviceServiceCollection services;
        using(_ = _devices.ObtainWriteLock(out var devices)) {
            if(!devices.TryGetValue(device, out var deviceInfo)) {
                _consoleOutput.ErrorLine($"Got Device Action from unknown device '{device.Name}'.");
                return;
            }

            services = deviceInfo.Services;
            newStatus = services.ExtractStatus(data);
            statusUpdated = !deviceInfo.Status.CompareEqual(newStatus);
            if(statusUpdated)
                deviceInfo.Status = newStatus;
        }

        if(statusUpdated)
            _guiApp.DeviceStateUpdated(device.Address, newStatus);

        var internalEvents = services.ProcessExternalEvent(device, data);
        RouteInternalEvents(internalEvents);

        _consoleOutput.ErrorLine($"Device {device.Name} action: {string.Join(", ", data.Select(kv => $"{kv.Key}={kv.Value}"))}");
    }

    public Task PreviewDeviceOption(string address, string value, PreviewMode previewMode)
    {
        DeviceServiceCollection services;
        using (var _ = _devices.ObtainReadLock(out var devices)) {
            var device = devices.Keys.FirstOrDefault(entry => entry.Address == address);
            if(device == null) {
                _consoleOutput.ErrorLine($"Address '{address}' given by client does not match a device.");
                return Task.CompletedTask;
            }
            services = devices[device].Services;
        }

        services.PreviewDeviceOption(value, previewMode);
        return Task.CompletedTask;
    }


    /// Helper struct for function RouteInternalEvents to avoid locking two locks simultaneously.
    private readonly struct RouteInternalEvents_Workspace(InternalEvent @event, List<EventRoute>? routesFromSourceAddress)
    {
        internal InternalEvent Event { get; } = @event;
        internal List<EventRoute>? RoutesFromSourceAddress { get; } = routesFromSourceAddress;
        internal List<RouteInternalEvents_Workspace2> RoutesWithDestination { get; } = [];
    }
    private struct RouteInternalEvents_Workspace2
    {
        internal DeviceServiceCollection Services { get; set; }
        internal string TargetFunctionality { get; set; }
    }

    private void RouteInternalEvents(IEnumerable<InternalEvent> events)
    {
        var routes = RouteInternalEvents_GetEligibleRoutes(events);

        if (routes.Count == 0)
            return;

        AugmentRoutesWithDestination(routes);

        foreach (var entry in routes)
            foreach (var route in entry.RoutesWithDestination)
                route.Services.ProcessInternalEvent(entry.Event, route.TargetFunctionality);
    }

    private void AugmentRoutesWithDestination(List<RouteInternalEvents_Workspace> routes)
    {
        using var _ = _devices.ObtainReadLock(out var devices);

        foreach (var entry in routes) {
            Debug.Assert(entry.RoutesFromSourceAddress != null);
            foreach (var route in entry.RoutesFromSourceAddress) {
                foreach (var (targetDevice, targetInfo) in devices) {
                    if (targetDevice.Address != route.TargetAddress)
                        continue;

                    entry.RoutesWithDestination.Add(new RouteInternalEvents_Workspace2 {
                        Services = targetInfo.Services,
                        TargetFunctionality = route.TargetFunctionality,
                    });
                }
            }
        }
    }

    private List<RouteInternalEvents_Workspace> RouteInternalEvents_GetEligibleRoutes(IEnumerable<InternalEvent> events)
    {
        using var _ = _routes.ObtainReadLock(out var routes);
        return events.Select(ev => {
            if (!routes.TryGetValue(ev.SourceAddress, out var deviceRoutes))
                deviceRoutes = null;

            return new RouteInternalEvents_Workspace(ev, deviceRoutes?.Where(route => route.SourceEvent == ev.ServiceName).ToList());
        })
        .Where(el => el.RoutesFromSourceAddress != null && el.RoutesFromSourceAddress.Count > 0)
        .ToList();
    }

    private async void HandleDeviceDiscovered(IDevice device)
    {
        _consoleOutput.ErrorLine($"Discovered device {device.Address} ({device.Vendor} {device.Model}): {device.Description}");

        using(_ = _devices.ObtainWriteLock(out var devices)) {
            if(devices.ContainsKey(device))
                return; // Not new

            using var _ = _serviceOptionValues.ObtainReadLock(out var deviceServiceOptionValues);
            deviceServiceOptionValues.TryGetValue(device.Address, out var serviceOptionValues);

            devices.Add(device, CreateDeviceInfo(device, serviceOptionValues));
        }
        await _guiApp.DeviceListUpdated();
    }

    private async void HandleDeviceUpdated(IDevice device)
    {
        using(_ = _devices.ObtainWriteLock(out var devices)) {
            var existing = devices.Keys.FirstOrDefault(entry => entry.Address == device.Address);
            if (existing == default) {
                _consoleOutput.ErrorLine($"Error updating device. Device '{device.Address}' did not seem to exist.");
                return;
            }

            var data = devices[existing];
            devices.Remove(existing);
            devices[device] = data;
        }
        await _guiApp.DeviceListUpdated();
    }

    private DeviceInfo CreateDeviceInfo(IDevice device, IReadOnlyList<ServiceOptionValue>? serviceOptionValues)
    {
        return new DeviceInfo {
            Services = _deviceServiceMapping.GetServicesFor(device, serviceOptionValues)
        };
    }

    public async Task Run()
    {
        _consoleOutput.InfoLine("Controller running.");
        foreach(var bus in _deviceBuses)
            await bus.Connect();

        await _guiApp.Run();
        TerminateScheduleThread();
    }

    public IEnumerable<IEventRoute> GetRoutingFor(IDevice device)
    {
        using var _ = _routes.ObtainReadLock(out var routes);

        if(routes.TryGetValue(device.Address, out var result))
            return result;

        return [];
    }

    public IEnumerable<IDeviceScheduleEntry> GetScheduleFor(IDevice device)
    {
        using var _ = _schedules.ObtainReadLock(out var schedules);

        if(schedules.TryGetValue(device.Address, out var result))
            return result;

        return [];
    }

    public async Task SetDeviceOptions(string address, string name, IEnumerable<IEventRoute> routes, IDeviceScheduleEntry[] schedule, IServiceOptionValue[] serviceOptionValues)
    {
        IDevice device;
        List<ServiceOptionValue> savedServiceOptionValues;
        using(_devices.ObtainReadLock(out var devices)) {
            var kv = devices.FirstOrDefault(entry => entry.Key.Address == address);
            device = kv.Key;
            if(device == null) {
                _consoleOutput.ErrorLine($"Address '{address}' given by client does not match a device.");
                return;
            }

            var services = kv.Value.Services;
            savedServiceOptionValues = services.SetServiceOptionValues(serviceOptionValues);
        }

        await SetDeviceName(device, name);

        var newRoutes = ProcessDeviceRoutes(device, routes);
        var newSchedule = ProcessDeviceSchedule(device, schedule);

        using(_ = _serviceOptionValues.ObtainWriteLock(out var deviceServiceOptionValues))
            deviceServiceOptionValues[device.Address] = savedServiceOptionValues;

        using(_ = _routes.ObtainWriteLock(out var deviceRoutes))
            deviceRoutes[device.Address] = newRoutes;

        using(_ = _schedules.ObtainWriteLock(out var deviceSchedules))
            deviceSchedules[device.Address] = newSchedule;

        await SaveData();
        await _guiApp.DeviceDataUpdated(device);
    }


    private List<EventRoute> ProcessDeviceRoutes(IDevice device, IEnumerable<IEventRoute> routes)
    {
        var newRoutes = routes
            .Select(entry => new EventRoute(entry))
            .Where(entry => entry.Validate()) // Silently ignore any invalid configurations
            .ToList();

        if(newRoutes.Count == 0)
            _consoleOutput.InfoLine($"Clearing all routes for device {device.Address}.");
        else {
            _consoleOutput.InfoLine($"Setting routes for device {device.Address}:");
            foreach(var route in newRoutes)
                _consoleOutput.InfoLine($" - {route.SourceEvent} -> {route.TargetAddress}::{route.TargetFunctionality}");
        }

        return newRoutes;
    }


    private List<DeviceScheduleEntry> ProcessDeviceSchedule(IDevice device, IDeviceScheduleEntry[] schedules)
    {
        var eligibleActions = GetConsumableActionsFor(device);
        var newSchedules = schedules
            .Select(entry => new DeviceScheduleEntry(entry))
            .Where(entry => entry.Validate(eligibleActions)) // Silently ignore any invalid configurations
            .ToList();

        if(newSchedules.Count == 0)
            _consoleOutput.InfoLine($"Clearing all schedules for device {device.Address}.");
        else {
            _consoleOutput.InfoLine($"Setting schedules for device {device.Address}:");
            foreach(var schedule in newSchedules)
                _consoleOutput.InfoLine($" - {schedule}");
        }

        return newSchedules;
    }

    private static async Task SetDeviceName(IDevice device, string name)
    {
        if(device.Name == name)
            return;
        await device.SetName(name);
    }

    public async Task RequestOpenNetwork()
    {
        // TODO: The UI should make it possible to open up for each client individually. Also, the client should be able to indicate whether it supports this or not.
        var tasks = _deviceBuses.Select(bus => bus.RequestOpenNetwork(_openNetworkTimeSeconds)).ToArray();
        await Task.WhenAll(tasks);
    }

    private readonly ManualResetEvent _terminateEvent = new(false);
    public void TerminateScheduleThread()
    {
        _terminateEvent.Set();
        _scheduleThread.Join();
    }

    private void ScheduleRunner()
    {
        int _lastRunMinute = -1;
        var doTerminate = false;
        while(!doTerminate) {
            var now = DateTime.Now;
            if (now.Minute == _lastRunMinute) {
                var secondsToWait = 60 - now.Second + 1; // +1 for good measures
                doTerminate = _terminateEvent.WaitOne(secondsToWait * 1000);
                continue;
            }
            _lastRunMinute = now.Minute;

            var triggeredSchedules = GetTriggeredSchedules();
            ExecuteScheduleActions(triggeredSchedules);
        }
    }

    private void ExecuteScheduleActions(List<(string, DeviceScheduleEntry)> triggeredSchedules)
    {
        using var _ = _devices.ObtainReadLock(out var devices);
        foreach (var (address, entry) in triggeredSchedules) {
            var devInfo = devices.FirstOrDefault(kv => kv.Key.Address == address).Value;
            if(devInfo == null) {
                _consoleOutput.ErrorLine($"Device {address} not found when executing schedule action.");
                continue;
            }

            devInfo.Services.ProcessScheduleAction(entry.EventType, entry.Parameters);
        }
    }

    private List<(string, DeviceScheduleEntry)> GetTriggeredSchedules()
    {
        var now = DateTime.Now;
        var result = new List<(string, DeviceScheduleEntry)>();
        using var _ = _schedules.ObtainReadLock(out var schedules);
        foreach (var (address, entries) in schedules)
            foreach (var entry in entries)
                if (ShouldTrigger(entry.Trigger, now))
                    result.Add((address, entry));
        return result;
    }

    private static bool ShouldTrigger(IScheduleTrigger trigger, DateTime now)
    {
        int dayOfWeek = ((int)now.DayOfWeek + 6) % 7; // Shift to match the IScheduleTrigger days
        if(!trigger.Days.Contains(dayOfWeek))
            return false;

        return trigger.Time.Hour == now.Hour &&
            trigger.Time.Minute == now.Minute;
    }
}
