using LightAssistant;
using LightAssistant.Controller;
using LightAssistant.WebApi;

namespace LightAssistantOffline;

public static class OfflineApp {
    private static int result = 0;

    public static int Main(string[] args)
    {
        try {
            var defaultConfig = new Config();
            var consoleOutput = new ConsoleOutput() { Verbose = true };
            var mqttClient = new MqttEmulatedClient();
            var guiApp = new WebApi(consoleOutput, defaultConfig.WebApiHostAddress, defaultConfig.WebApiPort);
            var controller = new Controller(consoleOutput, mqttClient, guiApp, defaultConfig.DataPath, defaultConfig.OpenNetworkTimeSeconds);
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

