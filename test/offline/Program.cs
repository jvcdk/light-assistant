using LightAssistant;
using LightAssistant.Controller;
using LightAssistant.Interfaces;
using LightAssistant.WebApi;

namespace LightAssistantOffline;

public static class OfflineApp {
    private static int result = 0;

    public static int Main(string[] args)
    {
        try {
            var defaultConfig = new Config();
            var consoleOutput = new ConsoleOutput() { Verbose = true };
            var mqttClients = new List<IDeviceBus> {
                new MqttEmulatedClient(consoleOutput)
            };
            var guiApp = new WebApi(consoleOutput, defaultConfig.WebApiHostAddress, defaultConfig.WebApiPort);
            var controller = new Controller(consoleOutput, mqttClients, guiApp, new DummyDataStorage(), defaultConfig.OpenNetworkTimeSeconds);
            controller.Run().Wait();
            return result;
        }
        catch(Exception e) {
            Console.WriteLine($"Error: {e.Message}");
            result = 127;
        }
        return 0;
    }
}

class DummyDataStorage : Controller.IDataStorage
{
    public Controller.RunTimeData LoadData() => new();
    public Task SaveData(Controller.RunTimeData data) => Task.CompletedTask; // Do nothing
}
