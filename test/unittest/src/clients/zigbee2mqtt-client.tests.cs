using LightAssistant;
using LightAssistant.Clients;
using LightAssistant.Interfaces;

namespace unittest;

public class Zigbee2MqttClientTests
{
    private Zigbee2MqttClient? _client;
    private TestMqttConnection? _connection;
    private const string BASE_TOPIC = "zigbee2mqtt";

    [SetUp]
    public void Setup()
    {
        var consoleOutput = new ConsoleOutput() { Verbose = false };
        _connection = new TestMqttConnection();
        _client = new Zigbee2MqttClient(_connection, consoleOutput, BASE_TOPIC);
    }

    [Test]
    public void WhenRequestOpenNetwork_ThenMqttMessagePublished()
    {
        Assert.IsNotNull(_client);
        Assert.IsNotNull(_connection);

        _client.RequestOpenNetwork(30).Wait();

        Assert.That(_connection.LastPublishedTopic, Is.EqualTo(BASE_TOPIC + "/bridge/request/permit_join"));
        Assert.That(_connection.LastPublishedMessage, Is.EqualTo(@"{""time"":""30""}"));
    }

    [Test]
    public void ParseTest_PermitJoinResponse()
    {
        Assert.IsNotNull(_client);
        Assert.IsNotNull(_connection);
        Assert.IsTrue(_connection.HasCallback());

        bool callbackCalled = false;
        bool isOpen = false;
        int time = 0;
        void networkOpenCallback(bool _isOpen, int _time)
        {
            callbackCalled = true;
            isOpen = _isOpen;
            time = _time;
        }

        try {
            _client.NetworkOpenStatus += networkOpenCallback;

            var message = @"{""data"":{""time"":30},""status"":""ok""}";
            _connection.SimulateMessage(BASE_TOPIC + "/bridge/response/permit_join", message);
        }
        finally {
            _client.NetworkOpenStatus -= networkOpenCallback;
        }

        Assert.IsTrue(callbackCalled);
        Assert.IsTrue(isOpen);
        Assert.That(time, Is.EqualTo(30));
    }

    private class TestMqttConnection : IMqttConnection
    {
        public Task ConnectAsync() => Task.CompletedTask;
        public Task Publish(string topic, string message)
        {
            LastPublishedTopic = topic;
            LastPublishedMessage = message;
            return Task.CompletedTask;
        }
        public string LastPublishedTopic { get; private set; } = "";
        public string LastPublishedMessage { get; private set; } = "";

        public void SubscribeToTopic(string topic, Action<IReadOnlyList<string>, string> callback)
        {
            _callback = callback;
        }
        private Action<IReadOnlyList<string>, string> _callback = delegate { };
        internal bool HasCallback() => _callback != null;

        internal void SimulateMessage(string topic, string message)
        {
            var parts = topic.Split('/').AsReadOnly();
            _callback(parts, message);
        }
    }
}
