using Newtonsoft.Json;
namespace LightAssistant;

/// TODO: Re-write to use reflection to find properties.
internal class Config
{
    public string MqttHost { get; set; } = "localhost";
    public int MqttPort { get; set; } = 1883;

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

                default:
                    Console.Error.WriteLine($"Invalid configuration line: {line}");
                    break;
            }
        }

        Console.WriteLine($"Loaded configuration file {configFile}");
    }

    public void SaveToFile(string configFile)
    {
        var json = JsonConvert.SerializeObject(this, Formatting.Indented);
        File.WriteAllText(configFile, json);
    }
}
