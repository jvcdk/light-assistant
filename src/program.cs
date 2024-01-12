
using CommandLine;

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
    }

    public static int Main(string[] args)
    {
        int result = 0;
        Parser.Default.ParseArguments<Options>(args)
            .WithParsed(async options => result = await Run(options));
        return result;
    }

    private static async Task<int> Run(Options options)
    {
        if(string.IsNullOrEmpty(options.ConfigFile))
            options.ConfigFile = Environment.GetEnvironmentVariable(ENV_CONFIG_FILE_KEY) ?? ENV_CONFIG_FILE_DEFAULT;

        try {
            var config = new Config(options.ConfigFile);
            var consoleOutput = new ConsoleOutput() { Verbose = options.Verbose };
            var mqttBus = new MqttClient(consoleOutput, config.MqttHost, config.MqttPort, config.MqttBaseTopic, APP_NAME);
            await mqttBus.ConnectAsync();

        }
        catch(Exception e) {
            Console.WriteLine($"Error: {e.Message}");
            return 127;
        }

        return 0;
    }
}
