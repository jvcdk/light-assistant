using CommandLine;
using LightAssistant.Clients;
using LightAssistant.Interfaces;

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
            if(config.Clients.Count == 0) {
                Console.Error.WriteLine("No clients configured. Use --save-config to create a default configuration file.");
                result = 1;
                return;
            }

            var consoleOutput = new ConsoleOutput() { Verbose = options.Verbose };
            var clients = config.Clients.Select(clientConfig => CreateClientConnection(clientConfig, consoleOutput)).ToList();
            var guiApp = new WebApi.WebApi(consoleOutput, config.WebApiHostAddress, config.WebApiPort);
            var controller = new Controller.Controller(consoleOutput, clients, guiApp, new DataStorage(config.DataPath, consoleOutput), config.OpenNetworkTimeSeconds, new SystemUtils());

            await controller.Run();
        }
        catch(Exception e) {
            Console.WriteLine($"Error: {e.Message}");
            result = 127;
        }
    }

    private static readonly Dictionary<string, MqttConnection> _mqttConnections = new();

    private static MqttConnection GetMqttConnection(string host, int port, ConsoleOutput consoleOutput)
    {
        var key = $"{host}:{port}";
        if(_mqttConnections.TryGetValue(key, out MqttConnection? value))
            return value;

        value = new MqttConnection(consoleOutput, host, port, APP_NAME);
        _mqttConnections[key] = value;
        return value;
    }

    private static IDeviceBus CreateClientConnection(Config.ClientConnection config, ConsoleOutput consoleOutput)
    {
        var clientType = config.Type.ToLower();
        if(clientType == Config.ClientType.Zigbee2Mqtt) {
            var connection = GetMqttConnection(config.Host, config.Port, consoleOutput);
            return new Zigbee2MqttClient(connection, consoleOutput, config.BaseTopic);
        }
        else if(clientType == Config.ClientType.PiPwm) {
            var connection = GetMqttConnection(config.Host, config.Port, consoleOutput);
            return new PiPwmClient(connection, consoleOutput, config.BaseTopic);
        }
        else
            throw new ArgumentException($"Unknown client type: {config.Type}");
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
