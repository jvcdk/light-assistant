using Newtonsoft.Json;
namespace LightAssistant;

internal class Config
{
    public string MqttHost { get; set; } = "localhost";
    public int MqttPort { get; set; } = 1883;
    public bool Verbose { get; set; } = false;
    public string WebGuiHostAddress { get; set; } = "*";
    public int WebGuiPort { get; set; } = 8080;

    public Config(string configFile = "", bool configFileSpecifiedDirectly = false)
    {
        if (string.IsNullOrWhiteSpace(configFile))
            return;

        if(!File.Exists(configFile)) {
            if(configFileSpecifiedDirectly)
                throw new FileNotFoundException($"Configuration file {configFile} not found.");

            return;
        }

        LoadFromFile(configFile);
    }

    public void SaveToFile(string configFile)
    {
        var json = JsonConvert.SerializeObject(this, Formatting.Indented);
        File.WriteAllText(configFile, json);
        Console.WriteLine($"Saved default configuration file {configFile}");
    }

    private void LoadFromFile(string configFile)
    {
        var json = File.ReadAllText(configFile);
        JsonConvert.PopulateObject(json, this);
        Console.WriteLine($"Loaded configuration file {configFile}");
    }
}
