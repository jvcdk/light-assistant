
using LightAssistant.Interfaces;
using Newtonsoft.Json;

namespace LightAssistant.Zigbee;

internal partial class Zigbee2MqttClient : IDeviceBusConnection
{
    private const string BASE_TOPIC = "zigbee2mqtt";

    public event Action<IDevice> DeviceDiscovered = (device) => { };
    public event Action<IDevice, Dictionary<string, string>> DeviceAction = (device, action) => { };

    private IConsoleOutput _consoleOutput;
    private ZigbeeConnection _connection;
    private readonly List<IDevice> _knownDevices = new();

    internal Zigbee2MqttClient(ZigbeeConnection connection, IConsoleOutput consoleOutput)
    {
        _consoleOutput = consoleOutput;
        _connection = connection;
        connection.SubscribeToTopic($"{BASE_TOPIC}", HandleMessage);
    }

    private void HandleMessage(IReadOnlyCollection<string> topics, string message)
    {
        if(topics.Count == 0) {
            _consoleOutput.ErrorLine($"Error: Unexpected empty in {nameof(Zigbee2MqttClient)}.");
            return;
        }

        if(topics.Count < 2 || topics.First() != BASE_TOPIC) {
            _consoleOutput.ErrorLine($"Error: Unexpected topic {string.Join('/', topics)} in {nameof(Zigbee2MqttClient)}.");
            return;
        }

        var deviceAddr = topics.ElementAt(1);
        switch(deviceAddr) {
            case "bridge":
                HandleBridgeMessage(topics.Skip(2).ToList(), message);
                break;
            default:
                var device = _knownDevices.FirstOrDefault(d => d.Address == deviceAddr);
                if (device != null)
                    HandleDeviceMessage(deviceAddr, message);
                else
                    _consoleOutput.ErrorLine($"Error: Unexpected device {deviceAddr} in {nameof(Zigbee2MqttClient)}.");

                break;
        }
    }

    private void HandleDeviceMessage(string deviceAddr, string message)
    {
        var device = _knownDevices.FirstOrDefault(d => d.Address == deviceAddr);
        if(device == null) {
            _consoleOutput.ErrorLine($"Error: Unexpected device {deviceAddr} in {nameof(Zigbee2MqttClient)}.");
            return;
        }

        var deviceMessage = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);
        if(deviceMessage == null) {
            _consoleOutput.ErrorLine($"Error: Could not parse device message in {nameof(Zigbee2MqttClient)}. Message: {message}.");
            return;
        }

        if(deviceMessage.ContainsKey("action")) {
            DeviceAction(device, deviceMessage);
            return;
        }

        _consoleOutput.ErrorLine($"Device message. Device: {deviceAddr}. Message: {message}");
    }

    private void HandleBridgeMessage(IReadOnlyList<string> list, string message)
    {
        if(list.Count == 0) {
            _consoleOutput.ErrorLine($"Error: Unexpected empty in {nameof(Zigbee2MqttClient)}.");
            return;
        }

        var command = list.First();
        switch(command) {
            case "devices":
                HandleBridgeDevicesMessage(message);
                break;
            case "groups":
            case "info":
            case "state":
            case "extensions":
            case "logging":
                HandleBridgeIgnoredMessage(command, message);
                break;
            default:
                _consoleOutput.ErrorLine($"Error: Unexpected bridge command {command} in {nameof(Zigbee2MqttClient)}.");
                break;
        }
    }

    private void HandleBridgeIgnoredMessage(string command, string message)
    {
        _consoleOutput.InfoLine($"Ignored Bridge message. Command: {command}. Message: {message}");
    }

    private void HandleBridgeDevicesMessage(string message)
    {
        try {
            var devices = JsonConvert.DeserializeObject<List<Device>>(message);
            if(devices == null) {
                _consoleOutput.ErrorLine($"Error: Unexpected empty device list in {nameof(Zigbee2MqttClient)}.");
                return;
            }
            foreach(var device in devices) {
                DeviceDiscovered(device);
                var existingDevice = _knownDevices.FirstOrDefault(d => d.Address == device.Address);
                if(existingDevice == null)
                    _knownDevices.Add(device);
            }
        }
        catch(Exception e) {
            _consoleOutput.ErrorLine($"Error: {e.Message}");
        }
    }
}
