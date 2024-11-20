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

    // Protected data
    private readonly Dictionary<IDevice, DeviceInfo> _devices = [];
    private readonly SlimReadWriteLock _devicesLock = new();
    private readonly Dictionary<string, List<EventRoute>> _routes = [];
    private readonly Dictionary<string, List<DeviceScheduleEntry>> _schedules = [];
    private readonly SlimReadWriteLock _routesLock = new();

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
    }

    private async Task SaveData()
    {
        if(string.IsNullOrWhiteSpace(_dataPath))
            return; // Error written in LoadData

        try {
            RunTimeData data;
            using(var _ = _routesLock.ObtainReadLock()) {
                data = new RunTimeData(_routes, _schedules);
            }
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

        using var _ = _routesLock.ObtainWriteLock();
        data.PopulateRoutes(_routes);
    }

    public IReadOnlyList<IDevice> GetDeviceList()
    {
        using var _ = _devicesLock.ObtainReadLock();
        return new List<IDevice>(_devices.Keys);
    }

    public IDevice? TryGetDevice(string address)
    {
        using var _ = _devicesLock.ObtainReadLock();
        return _devices.Keys.FirstOrDefault(entry => entry.Address == address);
    }

    public bool TryGetDeviceStatus(IDevice device, out IDeviceStatus? status)
    {
        using var _ = _devicesLock.ObtainReadLock();

        if(_devices.TryGetValue(device, out var entry)) {
            status = entry.Status;
            return true;
        }

        status = null;
        return false;
    }

    public IRoutingOptions? GetRoutingOptionsFor(IDevice device)
    {
        using var _ = _devicesLock.ObtainReadLock();

        if(!_devices.TryGetValue(device, out var deviceInfo)) {
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

    public IReadOnlyList<IConsumableTrigger> GetConsumableTriggersFor(IDevice device)
    {
        using var _ = _devicesLock.ObtainReadLock();

        if(!_devices.TryGetValue(device, out var deviceInfo)) {
            _consoleOutput.ErrorLine($"Device not found. Name: {device.Name}");
            return [];
        }
        return deviceInfo.Services.ConsumedTriggers
            .Select(ev => new ConsumableTrigger(ev.Name, ev.Params))
            .ToList();
    }

    private void HandleDeviceAction(IDevice device, Dictionary<string, string> data)
    {
        bool didUpdate;
        DeviceStatus newStatus;
        DeviceServiceCollection services;
        using(var _ = _devicesLock.ObtainWriteLock()) {
            if(!_devices.TryGetValue(device, out var deviceInfo)) {
                _consoleOutput.ErrorLine($"Got Device Action from unknown device '{device.Name}'.");
                return;            
            }

            services = deviceInfo.Services;

            newStatus = deviceInfo.Status.Clone(); // Clone to avoid race conditions
            didUpdate = newStatus.UpdateFrom(data);
            if(didUpdate)
                deviceInfo.Status = newStatus;
        }

        if(didUpdate)
            _guiApp.DeviceStateUpdated(device.Address, newStatus);

        var internalEvents = services.ProcessExternalEvent(device, data);
        RouteInternalEvents(internalEvents);

        _consoleOutput.ErrorLine($"Device {device.Name} action: {string.Join(", ", data.Select(kv => $"{kv.Key}={kv.Value}"))}");
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
        using var _ = _devicesLock.ObtainReadLock();

        foreach (var entry in routes) {
            Debug.Assert(entry.RoutesFromSourceAddress != null);
            foreach (var route in entry.RoutesFromSourceAddress) {
                foreach (var (targetDevice, targetInfo) in _devices) {
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
        using var _ = _routesLock.ObtainReadLock();
        return events.Select(ev => {
            if (!_routes.TryGetValue(ev.SourceAddress, out var routes))
                routes = null;

            return new RouteInternalEvents_Workspace(ev, routes?.Where(route => route.SourceEvent == ev.ServiceName).ToList());
        })
        .Where(el => el.RoutesFromSourceAddress != null && el.RoutesFromSourceAddress.Count > 0)
        .ToList();
    }

    private async void HandleDeviceDiscovered(IDevice device)
    {
        _consoleOutput.ErrorLine($"Discovered device {device.Address} ({device.Vendor} {device.Model}): {device.Description}");
        using(var _ = _devicesLock.ObtainWriteLock()) {
            if(_devices.ContainsKey(device))
                return; // Not new

            _devices.Add(device, CreateDeviceInfo(device));
        }
        await _guiApp.DeviceListUpdated();
    }

    private async void HandleDeviceUpdated(IDevice device)
    {
        using(var _ = _devicesLock.ObtainWriteLock()) {
            var existing = _devices.Keys.FirstOrDefault(entry => entry.Address == device.Address);
            if (existing == default) {
                _consoleOutput.ErrorLine($"Error updating device. Device '{device.Address}' did not seem to exist.");
                return;
            }

            var data = _devices[existing];
            _devices.Remove(existing);
            _devices[device] = data;
        }
        await _guiApp.DeviceListUpdated();
    }

    private DeviceInfo CreateDeviceInfo(IDevice device)
    {
        return new DeviceInfo {
            Services = _deviceServiceMapping.GetServicesFor(device)
        };
    }

    public async Task Run()
    {
        _consoleOutput.InfoLine("Controller running.");
        foreach(var bus in _deviceBuses)
            await bus.Connect();

        await _guiApp.Run();
    }

    public IEnumerable<IEventRoute> GetRoutingFor(IDevice device)
    {
        using var _ = _routesLock.ObtainReadLock();

        if(_routes.TryGetValue(device.Address, out var result))
            return result;
        
        return [];
    }

    public async Task SetDeviceOptions(string address, string name, IEnumerable<IEventRoute> routes, IDeviceScheduleEntry[] schedule)
    {
        var device = TryGetDevice(address);
        if(device == null) {
            _consoleOutput.ErrorLine($"Address '{address}' given by client does not match a device.");
            return;
        }

        await SetDeviceName(device, name);

        var newRoutes = ProcessDeviceRoutes(device, routes);
        var newSchedule = ProcessDeviceSchedule(device, schedule);
        using(_ = _routesLock.ObtainWriteLock()) {
            _routes[device.Address] = newRoutes;
            _schedules[device.Address] = newSchedule;
        }

        await SaveData();
        await _guiApp.RoutingDataUpdated(device);
    }


    private List<EventRoute> ProcessDeviceRoutes(IDevice device, IEnumerable<IEventRoute> routes)
    {
        var newRoutes = routes
            .Where(entry => !string.IsNullOrWhiteSpace(entry.SourceEvent)) // Silently ignore any invalid configurations
            .Where(entry => !string.IsNullOrWhiteSpace(entry.TargetAddress)) // Silently ignore any invalid configurations
            .Where(entry => !string.IsNullOrWhiteSpace(entry.TargetFunctionality)) // Silently ignore any invalid configurations
            .Select(entry => new EventRoute(entry)).ToList();

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
        var newSchedules = schedules
            .Where(entry => !string.IsNullOrWhiteSpace(entry.EventType)) // Silently ignore any invalid configurations
            .Select(entry => new DeviceScheduleEntry(entry))
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

    private async Task SetDeviceName(IDevice device, string name)
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
}

internal class DeviceScheduleEntry
{
    public DeviceScheduleEntry(IDeviceScheduleEntry entry)
    {
        EventType = entry.EventType;
        //Parameters = entry.Parameters;
        //Trigger = entry.Trigger;
    }

    public string EventType { get; }

    public override string ToString() => $"Schedule: {EventType}";

}