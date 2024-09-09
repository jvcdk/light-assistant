using LightAssistant.Interfaces;
using LightAssistant.Utils;
using MQTTnet;
using MQTTnet.Client;

namespace LightAssistant.Clients;

internal class MqttConnection : IDisposable
{
    private const int MQTT_TIMEOUT = 1500;

    private IMqttClient? _mqttClient;
    private MqttFactory? _mqttFactory;
    private IConsoleOutput ConsoleOutput { get; }
    private string Host { get; }
    private int Port { get; }
    private string ClientId { get; }
    private readonly InitGuard _connectedCalled = new();

    private readonly Dictionary<string, Action<IReadOnlyList<string>, string>> _subscriptions = [];

    internal MqttConnection(IConsoleOutput consoleOutput, string host, int port, string clientId)
    {
        ConsoleOutput = consoleOutput;
        Host = host;
        Port = port;
        ClientId = clientId;
    }

    public async Task ConnectAsync()
    {
        if(_connectedCalled.Check())
            return;

        _mqttFactory = new MqttFactory();
        var mqttClientOptions = new MqttClientOptionsBuilder()
            .WithClientId(ClientId)
            .WithTcpServer(Host, Port)
            .WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V500)
            .Build();
        
        _mqttClient = _mqttFactory.CreateMqttClient();
        var ct = new CancellationTokenSource(MQTT_TIMEOUT).Token;
        var response = await _mqttClient.ConnectAsync(mqttClientOptions, ct);

        if(!response.ResultCode.Equals(MqttClientConnectResultCode.Success))
            throw new Exception($"Failed to connect to MQTT server {Host}:{Port}");

        _mqttClient.ApplicationMessageReceivedAsync += HandleMqttMessage;

        var mqttSubscribeOptions = _mqttFactory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(f => f.WithTopic($"#").WithNoLocal())
                .Build();

        await _mqttClient.SubscribeAsync(mqttSubscribeOptions, ct);

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

    internal void SubscribeToTopic(string topic, Action<IReadOnlyList<string>, string> callback)
    {
        _subscriptions[topic] = callback;
    }

    internal async Task Publish(string topic, string message)
    {
        if(_mqttClient == null)
            throw new InvalidOperationException("Not connected to MQTT server");

        var applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(message)
                .Build();

        await _mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
    }

    public async void Dispose()
    {
        if(_mqttFactory == null || _mqttClient == null)
            return;

        var mqttClientDisconnectOptions = _mqttFactory
            .CreateClientDisconnectOptionsBuilder()
            .Build();
        await _mqttClient.DisconnectAsync(mqttClientDisconnectOptions, CancellationToken.None);

        _mqttClient.Dispose();
        _mqttClient = null;
        _mqttFactory = null;
    }
}
