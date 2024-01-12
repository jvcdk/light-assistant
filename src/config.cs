
namespace LightAssistant;

internal class Config
{
    public string MqttHost { get; set; } = "localhost";
    public int MqttPort { get; set; } = 1883;
    public string MqttBaseTopic { get; set; } = "zigbee2mqtt";

    public Config(string configFile = "", bool configFileSpecifiedDirectly = false)
    {
        if (string.IsNullOrWhiteSpace(configFile))
            return;

        if(!File.Exists(configFile)) {
            if(configFileSpecifiedDirectly)
                throw new FileNotFoundException($"Configuration file {configFile} not found.");

            return;
        }

        string[] lines = File.ReadAllLines(configFile);
        foreach (string line in lines) {
            if (line.StartsWith("//") || line.StartsWith('#'))
                continue;

            var idx = line.IndexOf(':');
            if (idx == -1) {
                Console.Error.WriteLine($"Invalid configuration line: {line}");
                continue;
            }

            var (key, value) = (line.Substring(0, idx).Trim().ToLower(), line.Substring(idx + 1).Trim());

            switch (key) {
                case "mqtt_host":
                    MqttHost = value;
                    break;

                case "mqtt_port" when int.TryParse(value, out int port):
                    MqttPort = port;
                    break;

                case "mqtt_base_topic":
                    MqttBaseTopic = value;
                    break;

                default:
                    Console.Error.WriteLine($"Invalid configuration line: {line}");
                    break;
            }
        }

        Console.WriteLine($"Loaded configuration file {configFile}");
    }
}
