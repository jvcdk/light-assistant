using LightAssistant.Interfaces;
using MQTTnet;
using MQTTnet.Client;

namespace LightAssistant.Zigbee;

internal class ZigbeeConnection : IDeviceBus, IDisposable
{
    private const int MQTT_TIMEOUT = 1500;

    private IMqttClient? mqttClient;
    private MqttFactory? mqttFactory;
    private IConsoleOutput ConsoleOutput { get; }
    private string Host { get; }
    private int Port { get; }
    private string ClientId { get; }

    private readonly Dictionary<string, Action<IReadOnlyCollection<string>, string>> _subscriptions = new();

    internal ZigbeeConnection(IConsoleOutput consoleOutput, string host, int port, string clientId)
    {
        ConsoleOutput = consoleOutput;
        Host = host;
        Port = port;
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
        var ct = new CancellationTokenSource(MQTT_TIMEOUT).Token;
        var response = await mqttClient.ConnectAsync(mqttClientOptions, ct);

        if(!response.ResultCode.Equals(MqttClientConnectResultCode.Success))
            throw new Exception($"Failed to connect to MQTT server {Host}:{Port}");

        mqttClient.ApplicationMessageReceivedAsync += HandleMqttMessage;

        var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(f => f.WithTopic($"#"))
                .Build();

        await mqttClient.SubscribeAsync(mqttSubscribeOptions, ct);

        ConsoleOutput.MessageLine($"Connected to MQTT server {Host}:{Port}");
    }

    private Task HandleMqttMessage(MqttApplicationMessageReceivedEventArgs e)
    {
        var handlers = _subscriptions
            .Where(kv => e.ApplicationMessage.Topic.StartsWith(kv.Key))
            .ToList();

        if(handlers.Count == 0)
            ConsoleOutput.ErrorLine($"No handler for topic {e.ApplicationMessage.Topic}.");
        else {
            var parts = e.ApplicationMessage.Topic.Split('/').AsReadOnly();
            var payload = e.ApplicationMessage.ConvertPayloadToString();
            foreach(var handler in handlers.Select(kv => kv.Value))
                handler(parts, payload);
        }

        return Task.CompletedTask;
    }

    internal void SubscribeToTopic(string topic, Action<IReadOnlyCollection<string>, string> callback)
    {
        _subscriptions[topic] = callback;
    }

//    private void Publish(string topic, string message)
//    {
//        if(mqttClient == null)
//            throw new InvalidOperationException("Not connected to MQTT server");
//
//        var applicationMessage = new MqttApplicationMessageBuilder()
//                .WithTopic($"{BaseTopic}/{topic}")
//                .WithPayload(message)
//                .Build();
//
//        // Fire and forget
//        mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
//    }

//    public async Task Subscribe(string topic, Action<string, string> callback)
//    {
//        if(mqttClient == null || mqttFactory == null)
//            throw new InvalidOperationException("Not connected to MQTT server");
//
//        var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
//                .WithTopicFilter(f => f.WithTopic($"{BaseTopic}/{topic}"))
//                .Build();
//
//        _subscriptions[$"{BaseTopic}/{topic}"] = callback;
//        await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);
//    }

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
