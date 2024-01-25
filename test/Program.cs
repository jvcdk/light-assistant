using LightAssistant;
using LightAssistant.Zigbee;
using LightAssistant.WebGUI;

namespace LightAssistantOffline;

public static class OfflineApp {
    private const string WebGuiHostAddress = "*";
    private const int WebGuiHostPort = 8080;
    private static int result = 0;

    public static int Main(string[] args)
    {
        try {
            var consoleOutput = new ConsoleOutput() { Verbose = true };
            var zigbeeConnection = new ZigbeeConnection(consoleOutput, "localhost", 1889, "app name");
            var mqttClient = new Zigbee2MqttClient(zigbeeConnection, consoleOutput);
            var guiApp = new App(consoleOutput, WebGuiHostAddress, WebGuiHostPort);
            var controller = new Controller(consoleOutput, mqttClient, guiApp);
            zigbeeConnection.ConnectAsync().Wait();
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

