using LightAssistant.Interfaces;

namespace LightAssistant;

internal class Controller : IController
{
    private IConsoleOutput ConsoleOutput { get; }
    private IDeviceBus DeviceBus { get; }

    public Controller(IConsoleOutput consoleOutput, IDeviceBus deviceBus)
    {
        ConsoleOutput = consoleOutput;
        DeviceBus = deviceBus;
    }

    public async Task Run()
    {
        ConsoleOutput.InfoLine("Controller running.");
        await DeviceBus.Subscribe("#", ProcessIncomingMessage); // Subscribe once to everything
        await Task.Delay(Timeout.Infinite);
    }

    private void ProcessIncomingMessage(string topic, string message)
    {
        // Do someting...
    }
}
