using LightAssistant;
using LightAssistant.WebApi;

namespace LightAssistantOffline;

public static class OfflineApp {
    private const string WebApiHostAddress = "*";
    private const int WebApiHostPort = 8081;
    private static int result = 0;
    private const string DeviceMappingFile = ".deviceMapping";

    public static int Main(string[] args)
    {
        try {
            var consoleOutput = new ConsoleOutput() { Verbose = true };
            var mqttClient = new MqttEmulatedClient();
            var guiApp = new WebApi(consoleOutput, WebApiHostAddress, WebApiHostPort);
            var controller = new Controller(consoleOutput, mqttClient, guiApp, DeviceMappingFile);
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

