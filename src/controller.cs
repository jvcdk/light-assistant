using LightAssistant.Interfaces;

namespace LightAssistant;

internal class Controller : IController
{
    private IConsoleOutput ConsoleOutput { get; }
    private IDeviceBusConnection DeviceBus { get; }

    public Controller(IConsoleOutput consoleOutput, IDeviceBusConnection deviceBus)
    {
        ConsoleOutput = consoleOutput;
        DeviceBus = deviceBus;

        DeviceBus.DeviceDiscovered += HandleDeviceDiscovered;
        DeviceBus.DeviceAction += HandleDeviceAction;
    }

    private void HandleDeviceAction(IDevice device, Dictionary<string, string> dictionary)
    {
        ConsoleOutput.ErrorLine($"Device {device.Name} action: {string.Join(", ", dictionary.Select(kv => $"{kv.Key}={kv.Value}"))}");
    }

    private void HandleDeviceDiscovered(IDevice device)
    {
        ConsoleOutput.ErrorLine($"Discovered device {device.Address} ({device.Vendor} {device.Model}): {device.Description}");
    }

    public async Task Run()
    {
        ConsoleOutput.InfoLine("Controller running.");
        await Task.Delay(Timeout.Infinite);
    }
}
