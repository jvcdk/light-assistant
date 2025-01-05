
using Newtonsoft.Json;
using LightAssistant.Interfaces;

namespace LightAssistant.Clients;

internal partial class PiPwmClient : IDeviceBus
{
    public event Action<IDevice> DeviceDiscovered = delegate { };
    public event Action<IDevice> DeviceUpdated = delegate { };
    public event Action<IDevice, Dictionary<string, string>> DeviceAction = delegate { };
    public event Action<bool, int> NetworkOpenStatus = delegate { };

    private readonly IConsoleOutput _consoleOutput;
    private readonly MqttConnection _connection;
    private readonly string _baseTopic;
    private readonly List<Device> _knownDevices = [];

    internal PiPwmClient(MqttConnection connection, IConsoleOutput consoleOutput, string baseTopic)
    {
        _consoleOutput = consoleOutput;
        _connection = connection;
        _baseTopic = baseTopic;
        connection.SubscribeToTopic($"{_baseTopic}", HandleMessage);
    }

    private void HandleMessage(IReadOnlyList<string> topics, string message)
    {
        // Sometimes mqtt gives an empty message. I suspect this is an artifact of suppressing
        // the re-broadcast of client-sent messages. That is; the NoLocal feature of mqtt.
        if(string.IsNullOrWhiteSpace(message))
            return;

        if(topics.Count == 0) {
            _consoleOutput.ErrorLine($"Error: Unexpected empty in {nameof(PiPwmClient)}.");
            return;
        }

        if(topics.Count < 2 || topics[0] != _baseTopic) {
            _consoleOutput.ErrorLine($"Error: Unexpected topic {string.Join('/', topics)} in {nameof(PiPwmClient)}.");
            return;
        }

        var deviceName = topics[1];
        var additionalTopics = topics.Skip(2).ToList();
        var device = _knownDevices.FirstOrDefault(d => d.Name == deviceName || d.Address == deviceName);
        if (device != null)
            HandleDeviceMessage(device, additionalTopics, message);
        else if(HandleDeviceDiscovery(deviceName, additionalTopics, message))
            return;
        else
            _consoleOutput.ErrorLine($"Error: Unexpected device {deviceName} in {nameof(PiPwmClient)}.");
    }

    private bool HandleDeviceDiscovery(string deviceId, IReadOnlyList<string> topics, string message)
    {
        if(topics.Count == 0)
            return false;

        var command = topics[0];
        if(command != "identity")
            return false;

        var device = new Device(_consoleOutput) {
            Name = deviceId,
            Address = deviceId,
            SendToBus = SendDataToDevice
        };
        _knownDevices.Add(device);
        DeviceDiscovered(device);
        HandleDeviceMessage(device, topics, message);
        return true;
    }

    private void HandleDeviceMessage(Device device, IReadOnlyList<string> topics, string message)
    {
        if(topics.Count == 0) {
            _consoleOutput.ErrorLine($"Unexpected empty command.");
            return;
        }

        Dictionary<string, string>? deviceMessage;
        try {
            var messageObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(message);
            deviceMessage = messageObj?.ToDictionary(kv => kv.Key, kv => kv.Value.ToString() ?? "");
        }
        catch(Exception ex) {
            _consoleOutput.ErrorLine($"Error: Could not parse device message in {nameof(PiPwmClient)}. Topics: '{string.Join('/', topics)}'. Mqtt Message: {message}. Exception: {ex.Message}");
            return;
        }
        if(deviceMessage == null) {
            _consoleOutput.ErrorLine($"Error: Parsed device message in {nameof(PiPwmClient)} was null. Mqtt Message: {message}.");
            return;
        }

        var command = topics[0];
        var additionalTopics = topics.Skip(1).ToList();
        switch(command) {
            case "identity":
                HandleDeviceIdentity(device, deviceMessage);
                break;

            case "status":
                HandleDeviceState(device, deviceMessage);
                break;

            default:
                _consoleOutput.ErrorLine($"Error: Unhandled device command '{string.Join('/', topics)}' => '{message}' in {nameof(PiPwmClient)}.");
                break;
        }
    }

    private void HandleDeviceState(IDevice device, Dictionary<string, string> deviceMessage)
    {
        DeviceAction(device, deviceMessage);
    }

    private void HandleDeviceIdentity(Device device, Dictionary<string, string> deviceMessage)
    {
        if(deviceMessage.TryGetValue("name", out var name))
            device.Name = name;

        DeviceUpdated(device);
    }

    private async Task SendDataToDevice(string path, Dictionary<string, string> data)
    {
        var message = JsonConvert.SerializeObject(data);
        await _connection.Publish($"{_baseTopic}/{path}", message);
    }

    public Task RequestOpenNetwork(int openNetworkTimeSeconds) => Task.CompletedTask; // Do nothing - does not support network open.

    public async Task Connect() => await _connection.ConnectAsync();
}
