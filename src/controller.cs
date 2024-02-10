using LightAssistant.Interfaces;

namespace LightAssistant;

internal partial class Controller : IController
{
    private readonly IConsoleOutput _consoleOutput;
    private readonly IDeviceBusConnection _deviceBus;
    private readonly IUserInterface _guiApp;
    private readonly Dictionary<IDevice, DeviceStatus> _devices = new();

    public IReadOnlyList<IDevice> GetDeviceList()
    {
        lock(_devices)
            return new List<IDevice>(_devices.Keys);
    }

    public bool TryGetDeviceStatus(IDevice device, out IDeviceStatus? status)
    {
        lock(_devices) {
            if(_devices.TryGetValue(device, out var entry)) {
                status = entry;
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

        _deviceBus.DeviceDiscovered += HandleDeviceDiscovered;
        _deviceBus.DeviceAction += HandleDeviceAction;
        _guiApp.AppController = this;
    }

    private void HandleDeviceAction(IDevice device, Dictionary<string, string> state)
    {
        if(!_devices.TryGetValue(device, out var deviceStatus)) {
            _consoleOutput.ErrorLine($"Got Device Action from unknown device '{device.Name}'.");
            return;            
        }

        var didUpdate = deviceStatus.UpdateFrom(state);
        if(didUpdate)
            _guiApp.DeviceStateUpdated(device.Address, deviceStatus);

        _consoleOutput.ErrorLine($"Device {device.Name} action: {string.Join(", ", state.Select(kv => $"{kv.Key}={kv.Value}"))}");
    }

    private void HandleDeviceDiscovered(IDevice device)
    {
        bool deviceIsNew;
        lock(_devices) {
            deviceIsNew = !_devices.ContainsKey(device);
            if(deviceIsNew)
                _devices.Add(device, new DeviceStatus());
        }
        if(deviceIsNew)
            _guiApp.DeviceListUpdated();

        _consoleOutput.ErrorLine($"Discovered device {device.Address} ({device.Vendor} {device.Model}): {device.Description}");
    }

    public async Task Run()
    {
        _consoleOutput.InfoLine("Controller running.");
        await _guiApp.Run();
    }
}
