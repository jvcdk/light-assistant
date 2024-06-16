
using Newtonsoft.Json;
using LightAssistant.Interfaces;
using System.Diagnostics;

namespace LightAssistant.Zigbee;

internal partial class Zigbee2MqttClient : IDeviceBus
{
    private const string BASE_TOPIC = "zigbee2mqtt";

    public event Action<IDevice> DeviceDiscovered = delegate { };
    public event Action<IDevice> DeviceUpdated = delegate { };
    public event Action<IDevice, Dictionary<string, string>> DeviceAction = delegate { };
    public event Action<bool, int> NetworkOpenStatus = delegate { };

    private readonly IConsoleOutput _consoleOutput;
    private readonly ZigbeeConnection _connection;
    private readonly List<IDevice> _knownDevices = [];

    internal Zigbee2MqttClient(ZigbeeConnection connection, IConsoleOutput consoleOutput)
    {
        _consoleOutput = consoleOutput;
        _connection = connection;
        connection.SubscribeToTopic($"{BASE_TOPIC}", HandleMessage);
    }

    private void HandleMessage(IReadOnlyCollection<string> topics, string message)
    {
        // Sometimes mqtt gives an empty message. I suspect this is an artifact of suppressing
        // the re-broadcast of client-sent messages. That is; the NoLocal feature of mqtt.
        if(string.IsNullOrWhiteSpace(message))
            return;

        if(topics.Count == 0) {
            _consoleOutput.ErrorLine($"Error: Unexpected empty in {nameof(Zigbee2MqttClient)}.");
            return;
        }

        if(topics.Count < 2 || topics.First() != BASE_TOPIC) {
            _consoleOutput.ErrorLine($"Error: Unexpected topic {string.Join('/', topics)} in {nameof(Zigbee2MqttClient)}.");
            return;
        }

        var deviceName = topics.ElementAt(1);
        var additionalTopics = topics.Skip(2).ToList();
        switch(deviceName) {
            case "bridge":
                HandleBridgeMessage(additionalTopics, message);
                break;
            default:
                var device = _knownDevices.FirstOrDefault(d => d.Name == deviceName);
                if (device != null)
                    HandleDeviceMessage(device, additionalTopics, message);
                else
                    _consoleOutput.ErrorLine($"Error: Unexpected device {deviceName} in {nameof(Zigbee2MqttClient)}.");

                break;
        }
    }

    private void HandleDeviceMessage(IDevice device, List<string> additionalTopics, string message)
    {
        if(additionalTopics.Count != 0) {
            _consoleOutput.ErrorLine($"Unsupported: We don't yet support additional topics '{string.Join('/', additionalTopics)}'.");
            return;
        }

        Dictionary<string, string>? deviceMessage;
        try {
            deviceMessage = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);
        }
        catch(Exception ex) {
            _consoleOutput.ErrorLine($"Error: Could not parse device message in {nameof(Zigbee2MqttClient)}. Topics: '{string.Join('/', additionalTopics)}'. Mqtt Message: {message}. Exception: {ex.Message}");
            return;
        }
        if(deviceMessage == null) {
            _consoleOutput.ErrorLine($"Error: Parsed device message in {nameof(Zigbee2MqttClient)} was null. Mqtt Message: {message}.");
            return;
        }

        if(deviceMessage.ContainsKey("action")) {
            DeviceAction(device, deviceMessage);
            return;
        }

        _consoleOutput.ErrorLine($"Device message. Device: {device.Name}. Topics: '{string.Join('/', additionalTopics)}'. Message: {message}");
    }

    private void HandleBridgeMessage(IReadOnlyList<string> commandPath, string message)
    {
        if(commandPath.Count == 0) {
            _consoleOutput.ErrorLine($"Error: Unexpected empty in {nameof(Zigbee2MqttClient)}.");
            return;
        }

        var command = commandPath[0];
        switch(command) {
            case "devices":
                HandleBridgeDevicesMessage(message);
                break;

            case "response":
                HandleBridgeResponseMessage(commandPath.Skip(1).ToList(), message);
                break;

            case "groups":
            case "info":
            case "state":
            case "extensions":
            case "logging":
                HandleBridgeIgnoredMessage(command, message);
                break;

            default:
                _consoleOutput.ErrorLine($"Error: Unhandled bridge command '{string.Join('/', commandPath)}' => '{message}' in {nameof(Zigbee2MqttClient)}.");
                break;
        }
    }

    private void HandleBridgeResponseMessage(List<string> commands, string message)
    {
        if(commands.Count == 0) {
            _consoleOutput.ErrorLine($"Unexpected empty 'command' in response from mqtt.");
            return;
        }

        GenericMqttResponse? parsedMessage = null;
        try {
            parsedMessage = JsonConvert.DeserializeObject<GenericMqttResponse>(message);
        }
        catch(Exception ex) {
            _consoleOutput.ErrorLine($"Could not parse bridge response message. Error msg.: " + ex.Message);
            return;
        }

        if(parsedMessage == null) {
            _consoleOutput.ErrorLine($"Could not parse bridge response message. Message was empty.");
            return;
        }

        var command = commands[0];
        var additionalCommands = commands.Skip(1).ToList();
        switch(command) {
            case "device":
                HandleBridgeResponseDeviceMessage(additionalCommands, parsedMessage);
                return;
            
            case "permit_join":
                HandleBridgeResponsePermitJoinMessage(additionalCommands, parsedMessage);
                return;

        }

        _consoleOutput.ErrorLine($"Error: Unhandled bridge command '{string.Join('/', commands)}' => '{message}' in {nameof(HandleBridgeResponseDeviceMessage)}.");
    }

    private void HandleBridgeResponsePermitJoinMessage(List<string> additionalCommands, GenericMqttResponse parsedMessage)
    {
        if(parsedMessage.Status != "ok") {
            _consoleOutput.ErrorLine("Permit Join request failed.");
            return;
        }

        if(!parsedMessage.Data.TryGetValue("value", out var valueStr)) {
            _consoleOutput.ErrorLine("Permit Join request did not contain a 'value' parameter.");
            return;
        }

        if(!bool.TryParse(valueStr, out var value)) {
            _consoleOutput.ErrorLine("Permit Join request did not have a valid 'value' parameter.");
            return;
        }

        int time = 0;
        if(value) {
            if(!parsedMessage.Data.TryGetValue("time", out var timeStr)) {
                _consoleOutput.ErrorLine("Permit Join request did not contain a 'time' parameter.");
                return;
            }

            if(!int.TryParse(timeStr, out time)) {
                _consoleOutput.ErrorLine("Permit Join request did not have a valid 'time' parameter.");
                return;
            }
        }

        NetworkOpenStatus(value, time);
    }

    private void HandleBridgeResponseDeviceMessage(List<string> commands, GenericMqttResponse parsedMessage)
    {
        if(commands.Count == 0) {
            _consoleOutput.ErrorLine($"Unexpected empty 'command' in response from mqtt.");
            return;
        }

        var command = commands[0];
        switch(command) {
            case "rename":
                return; // Do nothing; the rename command arrives *after* new device enumeration list, so we already know about the rename
        }

        _consoleOutput.ErrorLine($"Error: Unhandled bridge command '{string.Join('/', commands)}' in {nameof(HandleBridgeResponseDeviceMessage)}.");
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
                IDevice? existingDevice = null;
                int idx;
                for(idx = 0; idx < _knownDevices.Count; idx++) {
                    existingDevice = _knownDevices[idx];
                    if(existingDevice.Address == device.Address)
                        break;
                }

                var foundExisting = idx < _knownDevices.Count;
                if(foundExisting) {
                    Debug.Assert(existingDevice != null);
                    if(!existingDevice.Equals(device)) {
                        _knownDevices[idx] = device;
                        DeviceUpdated(device);
                    }
                }
                else {
                    DeviceDiscovered(device);
                    _knownDevices.Add(device);
                }
            }
        }
        catch(Exception e) {
            _consoleOutput.ErrorLine($"Error: {e.Message}");
        }
    }

    public async Task SetDeviceName(string address, string name)
    {
        var data = new Dictionary<string, string> {
            ["from"] = address,
            ["to"] = name
        };
        var message = JsonConvert.SerializeObject(data);
        await _connection.Publish($"{BASE_TOPIC}/bridge/request/device/rename", message);
    }

    public async Task RequestOpenNetwork(int openNetworkTimeSeconds)
    {
        var data = new Dictionary<string, string> {
            ["value"] = "true",
            ["time"] = $"{openNetworkTimeSeconds}"
        };
        var message = JsonConvert.SerializeObject(data);
        await _connection.Publish($"{BASE_TOPIC}/bridge/request/permit_join", message);
    }
}
