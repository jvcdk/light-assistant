using Newtonsoft.Json;

namespace LightAssistant;

internal class Config
{
    public static class ClientType
    {
        public const string Zigbee2Mqtt = "zigbee2mqtt";
        public const string PiPwm = "pipwm";
    }

    public List<ClientConnection> Clients { get; set; } = [ClientConnection.DefaultZigbee2Mqtt, ClientConnection.DefaultPiPwm];
    public bool Verbose { get; set; } = false;
    public string WebApiHostAddress { get; set; } = "*";
    public int WebApiPort { get; set; } = 8081;
    public string DataPath { get; set; } = "light-assistant-data.json";
    public int OpenNetworkTimeSeconds { get; set; } = 30;

    public Config() { }

    public Config(string configFile, bool configFileSpecifiedDirectly = false)
    {
        if(!File.Exists(configFile)) {
            if(configFileSpecifiedDirectly)
                throw new FileNotFoundException($"Configuration file {configFile} not found.");

            return;
        }

        var json = File.ReadAllText(configFile);
        try {
            Clients.Clear();
            JsonConvert.PopulateObject(json, this);
            Console.Error.WriteLine($"Loaded configuration file {configFile}.");
        }
        catch(Exception ex) {
            Console.Error.WriteLine("Failed to read config file. Msg.: " + ex.Message);
            Console.Error.WriteLine("Using default values.");
        }
    }

    public void SaveToFile(string configFile)
    {
        var json = JsonConvert.SerializeObject(this, Formatting.Indented);
        File.WriteAllText(configFile, json);
        Console.Error.WriteLine($"Saved default configuration file {configFile}.");
    }

    public class ClientConnection
    {
        public required string Type { get; init; }
        public required string Host { get; init; }
        public required int Port { get; init; }
        public required string BaseTopic { get; init; }

        public static ClientConnection DefaultZigbee2Mqtt => new ClientConnection() {
            Type = ClientType.Zigbee2Mqtt,
            Host = "localhost",
            Port = 1883,
            BaseTopic = "zigbee2mqtt"
        };

        public static ClientConnection DefaultPiPwm => new ClientConnection() {
            Type = ClientType.PiPwm,
            Host = "localhost",
            Port = 1883,
            BaseTopic = "pipwm"
        };
    }
}
