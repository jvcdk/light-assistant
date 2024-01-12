
namespace LightAssistant
{

    internal class Config
    {
        public string MqttHost { get; set; } = "localhost";
        public int MqttPort { get; set; } = 1883;

        public Config(string configFile = "")
        {
            if (string.IsNullOrWhiteSpace(configFile))
                return;

            Dictionary<string, string> configDictionary = new Dictionary<string, string>();

            string[] lines = File.ReadAllLines(configFile);
            foreach (string line in lines) {
                if (line.StartsWith("//") || line.StartsWith('#'))
                    continue;

                var idx = line.IndexOf(':');
                if(idx == -1) {
                    Console.Error.WriteLine($"Invalid configuration line: {line}");
                    continue;
                }

                var key = line.Substring(0, idx).Trim().ToLower();
                var value = line.Substring(idx + 1).Trim();

                if(key == "mqtt_host") {
                    MqttHost = value;
                }
                else if(key == "mqtt_port") {
                    if(!int.TryParse(value, out int port)) {
                        Console.Error.WriteLine($"Invalid configuration line: {line}");
                        continue;
                    }
                    MqttPort = port;
                }
                else {
                    Console.Error.WriteLine($"Invalid configuration line: {line}");
                    continue;
                }
            }

            Console.WriteLine($"Loaded configuration file {configFile}");
        }
    }
}