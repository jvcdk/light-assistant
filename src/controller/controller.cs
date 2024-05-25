using System.Diagnostics;
using LightAssistant.Interfaces;
using LightAssistant.Utils;

namespace LightAssistant.Controller;

internal partial class Controller : IController
{
    private readonly IConsoleOutput _consoleOutput;
    private readonly IDeviceBusConnection _deviceBus;
    private readonly IUserInterface _guiApp;
    private readonly DeviceServiceMapping _deviceServiceMapping;

    // Protected data
    private readonly Dictionary<IDevice, DeviceInfo> _devices = [];
    private readonly SlimReadWriteLock _devicesLock = new();
    private readonly Dictionary<string, List<EventRoute>> _routes = [];
    private readonly SlimReadWriteLock _routesLock = new();

    public Controller(IConsoleOutput consoleOutput, IDeviceBusConnection deviceBus, IUserInterface guiApp)
    {
        _consoleOutput = consoleOutput;
        _deviceBus = deviceBus;
        _guiApp = guiApp;
        _deviceServiceMapping = new DeviceServiceMapping(_consoleOutput);

        _deviceBus.DeviceDiscovered += HandleDeviceDiscovered;
        _deviceBus.DeviceAction += HandleDeviceAction;
        _guiApp.AppController = this;

        // Temp - dummy TODO JVC: Implement interface to configure these; save them in a persistent file.
        _routes.Add("0x4c5bb3fffe2e8acb", [
            new EventRoute("Push", "target_1", "on/off"),
            new EventRoute("Toggle", "0x94deb8fffe6aa0be", "Flip")
        ]);
        _routes.Add("0x94deb8fffe6aa0be", [
            new EventRoute("Nope", "target_2", "brightness"),
        ]);
    }

    public IReadOnlyList<IDevice> GetDeviceList()
    {
        using var _ = _devicesLock.ObtainReadLock();
        return new List<IDevice>(_devices.Keys);
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

            return new RouteInternalEvents_Workspace(ev, routes?.Where(route => route.SourceEvent == ev.Type).ToList());
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

    private DeviceInfo CreateDeviceInfo(IDevice device)
    {
        return new DeviceInfo {
            Services = _deviceServiceMapping.GetServicesFor(device)
        };
    }

    public async Task Run()
    {
        _consoleOutput.InfoLine("Controller running.");
        await _guiApp.Run();
    }

    public IEnumerable<IEventRoute> GetRoutingFor(IDevice device)
    {
        using var _ = _routesLock.ObtainReadLock();

        if(_routes.TryGetValue(device.Address, out var result))
            return result;
        
        return [];
    }

    public async Task SetRoutingFor(IDevice device, IEnumerable<IEventRoute> routes)
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

        using(_ = _routesLock.ObtainWriteLock())
            _routes[device.Address] = newRoutes;

        // TODO JVC:
        // Save to Disk
        // Reply back
        await Task.CompletedTask;
    }
}
