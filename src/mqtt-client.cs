using LightAssistant.Interfaces;
using MQTTnet;
using MQTTnet.Client;

namespace LightAssistant;

internal class MqttClient : IDeviceBus, IDisposable
{
    private const int MQTT_TIMEOUT = 1500;

    private IMqttClient? mqttClient;
    private MqttFactory? mqttFactory;

    private Dictionary<string, Action<string, string>> _subscriptions = new();

    private IConsoleOutput ConsoleOutput { get; }
    private string Host { get; }
    private int Port { get; }
    private string BaseTopic { get; }
    private string ClientId { get; }

    internal MqttClient(IConsoleOutput consoleOutput, string host, int port, string baseTopic, string clientId)
    {
        ConsoleOutput = consoleOutput;
        Host = host;
        Port = port;
        BaseTopic = baseTopic;
        ClientId = clientId;
    }

    public async Task ConnectAsync()
    {
        mqttFactory = new MqttFactory();
        var mqttClientOptions = new MqttClientOptionsBuilder()
            .WithClientId(ClientId)
            .WithTcpServer(Host, Port)
            .Build();
        
        mqttClient = mqttFactory.CreateMqttClient();
        var response = await mqttClient.ConnectAsync(mqttClientOptions,
            new CancellationTokenSource(MQTT_TIMEOUT).Token);

        if(!response.ResultCode.Equals(MqttClientConnectResultCode.Success))
            throw new Exception($"Failed to connect to MQTT server {Host}:{Port}");

        mqttClient.ApplicationMessageReceivedAsync += e =>
        {
            ConsoleOutput.InfoLine($"Received message on topic {e.ApplicationMessage.Topic}, payload: {e.ApplicationMessage.ConvertPayloadToString()}");
            var msg = e.ApplicationMessage;
            foreach(var kv in _subscriptions.Where(kv => msg.Topic.StartsWith(kv.Key)))
                kv.Value(msg.Topic, msg.ConvertPayloadToString());

            return Task.CompletedTask;
        };

        ConsoleOutput.MessageLine($"Connected to MQTT server {Host}:{Port}");
    }

    public void Publish(string topic, string message)
    {
        if(mqttClient == null)
            throw new InvalidOperationException("Not connected to MQTT server");

        var applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic($"{BaseTopic}/{topic}")
                .WithPayload(message)
                .Build();

        // Fire and forget
        mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
    }

    public async Task Subscribe(string topic, Action<string, string> callback)
    {
        if(mqttClient == null || mqttFactory == null)
            throw new InvalidOperationException("Not connected to MQTT server");

        var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(f => f.WithTopic($"{BaseTopic}/{topic}"))
                .Build();

        _subscriptions[$"{BaseTopic}/{topic}"] = callback;
        await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);
    }

    public async void Dispose()
    {
        if(mqttFactory == null || mqttClient == null)
            return;

        var mqttClientDisconnectOptions = mqttFactory
            .CreateClientDisconnectOptionsBuilder()
            .Build();
        await mqttClient.DisconnectAsync(mqttClientDisconnectOptions, CancellationToken.None);

        mqttClient.Dispose();
        mqttClient = null;
        mqttFactory = null;
    }
}
