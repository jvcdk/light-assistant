
using CommandLine;
using LightAssistant.Zigbee;

namespace LightAssistant;

public static class MainApp {
    private const string APP_NAME = "LightAssistant";
    private const string ENV_CONFIG_FILE_KEY = "LIGHT_ASSISTANT_CONFIG";
    private const string ENV_CONFIG_FILE_DEFAULT = "light-assistant.conf";

    private class Options
    {
        [Option('c', "config", Required = false, HelpText = $"Configuration file. Overrides environment variable {ENV_CONFIG_FILE_KEY}. Default: {ENV_CONFIG_FILE_DEFAULT}.")]
        public string ConfigFile { get; set; } = "";

        [Option('v', "verbose", Required = false, HelpText = "Verbose output.")]
        public bool Verbose { get; set; } = false;

        [Option('s', "save-config", Required = false, HelpText = "Save default config file and exit.")]
        public bool SaveConfig { get; set; } = false;
    }

    private static int result = 0;

    public static int Main(string[] args)
    {
        Parser.Default.ParseArguments<Options>(args)
            .WithParsed(options => Run(options).Wait());
        return result;
    }

    private static async Task Run(Options options)
    {
        bool hasConfigFile = !string.IsNullOrEmpty(options.ConfigFile);
        if(!hasConfigFile) {
            var configFile = Environment.GetEnvironmentVariable(ENV_CONFIG_FILE_KEY);
            if(!string.IsNullOrEmpty(configFile)) {
                hasConfigFile = true;
                options.ConfigFile = configFile;
            }
            else
                options.ConfigFile = ENV_CONFIG_FILE_DEFAULT;
        }

        if(options.SaveConfig) {
            SaveConfigFile(options);
            return;
        }

        try {
            var config = new Config(options.ConfigFile, hasConfigFile);
            options.Verbose |= config.Verbose;
            var consoleOutput = new ConsoleOutput() { Verbose = options.Verbose };
            var zigbeeConnection = new ZigbeeConnection(consoleOutput, config.MqttHost, config.MqttPort, APP_NAME);
            var mqttClient = new Zigbee2MqttClient(zigbeeConnection, consoleOutput);
            var guiApp = new WebApi.WebApi(consoleOutput, config.WebApiHostAddress, config.WebApiPort);
            var controller = new Controller(consoleOutput, mqttClient, guiApp, config.DeviceMappingFile);
            await zigbeeConnection.ConnectAsync();
            await controller.Run();
        }
        catch(Exception e) {
            Console.WriteLine($"Error: {e.Message}");
            result = 127;
        }
    }

    private static void SaveConfigFile(Options options)
    {
        if(File.Exists(options.ConfigFile)) {
            Console.Error.WriteLine($"Configuration file {options.ConfigFile} already exists. Refusing to overwrite.");
            return;
        }
        var config = new Config();
        config.SaveToFile(options.ConfigFile);
    }
}
