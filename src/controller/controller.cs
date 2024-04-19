using LightAssistant.Interfaces;

namespace LightAssistant.Controller;

internal partial class Controller : IController
{
    private readonly IConsoleOutput _consoleOutput;
    private readonly IDeviceBusConnection _deviceBus;
    private readonly IUserInterface _guiApp;
    private readonly DeviceServiceMapping _deviceServiceMapping;
    private readonly Dictionary<IDevice, DeviceInfo> _devices = new();
    private readonly List<EventRoute> _routes = new();

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
        _routes.Add(new EventRoute("0x4c5bb3fffe2e8acb", "Push", "target_1", "on/off"));
        _routes.Add(new EventRoute("0x4c5bb3fffe2e8acb", "Toggle", "0x94deb8fffe6aa0be", "Flip"));
        _routes.Add(new EventRoute("0x94deb8fffe6aa0be", "Nope", "target_2", "brightness"));
    }

    public IReadOnlyList<IDevice> GetDeviceList()
    {
        lock(_devices)
            return new List<IDevice>(_devices.Keys);
    }

    public bool TryGetDeviceStatus(IDevice device, out IDeviceStatus? status)
    {
        lock(_devices) {
            if(_devices.TryGetValue(device, out var entry)) {
                status = entry.Status;
                return true;
            }
        }
        status = null;
        return false;
    }

    public IRoutingOptions? GetRoutingOptionsFor(IDevice device)
    {
        if(!_devices.TryGetValue(device, out var deviceInfo)) {
            _consoleOutput.ErrorLine($"Device not found. Name: {device.Name}");
            return null;
        }

        string GetInternalEventName(Type ev) => ev.Name.Replace("InternalEvent_", "");

        var providedEvents = deviceInfo.Services.ProvidedEvents
            .Select(ev => new ProvidedEvent(GetInternalEventName(ev.EventType), ev.Path))
            .ToList();
        var consumedEvents = deviceInfo.Services.ConsumedEvents
            .Select(ev => new ConsumableEvent(GetInternalEventName(ev.EventType), ev.TargetName))
            .ToList();
        return new RoutingOptions(providedEvents, consumedEvents);
    }

    private void HandleDeviceAction(IDevice device, Dictionary<string, string> data)
    {
        if(!_devices.TryGetValue(device, out var deviceInfo)) {
            _consoleOutput.ErrorLine($"Got Device Action from unknown device '{device.Name}'.");
            return;            
        }

        var didUpdate = deviceInfo.Status.UpdateFrom(data);
        if(didUpdate)
            _guiApp.DeviceStateUpdated(device.Address, deviceInfo.Status);

        var internalEvents = deviceInfo.Services.ProcessExternalEvent(device, data);
        RouteInternalEvents(internalEvents);

        _consoleOutput.ErrorLine($"Device {device.Name} action: {string.Join(", ", data.Select(kv => $"{kv.Key}={kv.Value}"))}");
    }

    private void RouteInternalEvents(IEnumerable<InternalEvent> events)
    {
        foreach(var ev in events) {
            foreach(var route in _routes.Where(route => route.SourceAddress == ev.SourceAddress && route.SourceEvent == ev.Type)) {
                foreach(var (targetDevice, targetInfo) in _devices) {
                    if(targetDevice.Address != route.TargetAddress)
                        continue;

                    targetInfo.Services.ProcessInternalEvent(ev, route.TargetFunctionality);
                }
            }
        }
    }

    private void HandleDeviceDiscovered(IDevice device)
    {
        bool deviceIsNew;
        lock(_devices) {
            deviceIsNew = !_devices.ContainsKey(device);
            if(deviceIsNew)
                _devices.Add(device, CreateDeviceInfo(device));
        }
        if(deviceIsNew)
            _guiApp.DeviceListUpdated();

        _consoleOutput.ErrorLine($"Discovered device {device.Address} ({device.Vendor} {device.Model}): {device.Description}");
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
        return _routes.Where(routing => routing.SourceAddress == device.Address);
    }
}
