using LightAssistant.Interfaces;

namespace LightAssistant;

internal class Controller : IController
{
    private readonly IConsoleOutput _consoleOutput;
    private readonly IDeviceBusConnection _deviceBus;
    private readonly IUserInterface _guiApp;

    public Controller(IConsoleOutput consoleOutput, IDeviceBusConnection deviceBus, IUserInterface guiApp)
    {
        _consoleOutput = consoleOutput;
        _deviceBus = deviceBus;
        _guiApp = guiApp;

        _deviceBus.DeviceDiscovered += HandleDeviceDiscovered;
        _deviceBus.DeviceAction += HandleDeviceAction;
    }

    private void HandleDeviceAction(IDevice device, Dictionary<string, string> dictionary)
    {
        _consoleOutput.ErrorLine($"Device {device.Name} action: {string.Join(", ", dictionary.Select(kv => $"{kv.Key}={kv.Value}"))}");
    }

    private void HandleDeviceDiscovered(IDevice device)
    {
        _consoleOutput.ErrorLine($"Discovered device {device.Address} ({device.Vendor} {device.Model}): {device.Description}");
    }

    public async Task Run()
    {
        _consoleOutput.InfoLine("Controller running.");
        await _guiApp.Run();
    }
}
