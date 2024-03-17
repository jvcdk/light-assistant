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

    public Controller(IConsoleOutput consoleOutput, IDeviceBusConnection deviceBus, IUserInterface guiApp)
    {
        _consoleOutput = consoleOutput;
        _deviceBus = deviceBus;
        _guiApp = guiApp;
        _deviceServiceMapping = new DeviceServiceMapping(_consoleOutput);

        _deviceBus.DeviceDiscovered += HandleDeviceDiscovered;
        _deviceBus.DeviceAction += HandleDeviceAction;
        _guiApp.AppController = this;
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

    private class EventRoute(string sourceAddress, string sourceEvent, string targetAddress, string targetFunctionality)
    {
        internal string SourceAddress { get; } = sourceAddress;
        internal string SourceEvent { get; } = sourceEvent;
        internal string TargetAddress { get; } = targetAddress;
        internal string TargetFunctionality { get; } = targetFunctionality;
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
}
