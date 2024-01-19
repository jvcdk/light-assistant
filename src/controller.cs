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

        DeviceBus.DeviceDiscovered += (sender, device) => HandleDeviceDiscovered(device);
    }

    private void HandleDeviceDiscovered(IDevice device)
    {
        ConsoleOutput.InfoLine($"Discovered device {device.Address} ({device.Vendor} {device.Model}): {device.Description}");
    }

    public async Task Run()
    {
        ConsoleOutput.InfoLine("Controller running.");
        await Task.Delay(Timeout.Infinite);
    }
}
