using LightAssistant.Interfaces;

namespace LightAssistant;

internal class Controller : IController
{
    private readonly IConsoleOutput _consoleOutput;
    private readonly IDeviceBusConnection _deviceBus;
    private readonly IUserInterface _guiApp;
    private readonly List<IDevice> _devices = new();

    public IReadOnlyList<IDevice> GetDeviceList()
    {
        lock(_devices)
            return new List<IDevice>(_devices);
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

    private void HandleDeviceAction(IDevice device, Dictionary<string, string> dictionary)
    {
        _consoleOutput.ErrorLine($"Device {device.Name} action: {string.Join(", ", dictionary.Select(kv => $"{kv.Key}={kv.Value}"))}");
    }

    private void HandleDeviceDiscovered(IDevice device)
    {
        bool deviceIsNew;
        lock(_devices) {
            var existing = _devices.FirstOrDefault(el => el == device);
            deviceIsNew = existing == default;
            if(deviceIsNew)
                _devices.Add(device);
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
