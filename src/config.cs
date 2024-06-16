using Newtonsoft.Json;

namespace LightAssistant;

internal class Config
{
    public string MqttHost { get; set; } = "localhost";
    public int MqttPort { get; set; } = 1883;
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

        Console.Error.WriteLine($"Loaded configuration file {configFile}.");
        var json = File.ReadAllText(configFile);
        try {
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
}
