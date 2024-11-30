
using Newtonsoft.Json;
using LightAssistant.Interfaces;
using System.Diagnostics;

namespace LightAssistant.Clients;

internal partial class Zigbee2MqttClient : IDeviceBus
{
    public event Action<IDevice> DeviceDiscovered = delegate { };
    public event Action<IDevice> DeviceUpdated = delegate { };
    public event Action<IDevice, Dictionary<string, string>> DeviceAction = delegate { };
    public event Action<bool, int> NetworkOpenStatus = delegate { };

    private readonly IConsoleOutput _consoleOutput;
    private readonly MqttConnection _connection;
    private readonly string _baseTopic;
    private readonly List<IDevice> _knownDevices = [];

    internal Zigbee2MqttClient(MqttConnection connection, IConsoleOutput consoleOutput, string baseTopic)
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
            _consoleOutput.ErrorLine($"Error: Unexpected empty in {nameof(Zigbee2MqttClient)}.");
            return;
        }

        if(topics.Count < 2 || topics[0] != _baseTopic) {
            _consoleOutput.ErrorLine($"Error: Unexpected topic {string.Join('/', topics)} in {nameof(Zigbee2MqttClient)}.");
            return;
        }

        var deviceName = topics[1];
        var additionalTopics = topics.Skip(2).ToList();
        switch(deviceName) {
            case "bridge":
                HandleBridgeMessage(additionalTopics, message);
                break;
            default:
                var device = _knownDevices.FirstOrDefault(d => d.Name == deviceName || d.Address == deviceName);
                if (device != null)
                    HandleDeviceMessage(device, additionalTopics, message);
                else
                    _consoleOutput.ErrorLine($"Error: Unexpected device {deviceName} in {nameof(Zigbee2MqttClient)}.");

                break;
        }
    }

    private void HandleDeviceMessage(IDevice device, IReadOnlyList<string> additionalTopics, string message)
    {
        if(additionalTopics.Count != 0) {
            _consoleOutput.ErrorLine($"Unsupported: We don't yet support additional topics '{string.Join('/', additionalTopics)}'.");
            return;
        }

        Dictionary<string, string>? deviceMessage;
        try {
            var messageObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(message);
            deviceMessage = messageObj?.ToDictionary(kv => kv.Key, kv => kv.Value.ToString() ?? "");
        }
        catch(Exception ex) {
            _consoleOutput.ErrorLine($"Error: Could not parse device message in {nameof(Zigbee2MqttClient)}. Topics: '{string.Join('/', additionalTopics)}'. Mqtt Message: {message}. Exception: {ex.Message}");
            return;
        }
        if(deviceMessage == null) {
            _consoleOutput.ErrorLine($"Error: Parsed device message in {nameof(Zigbee2MqttClient)} was null. Mqtt Message: {message}.");
            return;
        }

        DeviceAction(device, deviceMessage);
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
            case "definitions":
                HandleBridgeIgnoredMessage(command, message);
                break;

            default:
                _consoleOutput.ErrorLine($"Error: Unhandled bridge command '{string.Join('/', commandPath)}' => '{message}' in {nameof(Zigbee2MqttClient)}.");
                break;
        }
    }

    private void HandleBridgeResponseMessage(IReadOnlyList<string> commands, string message)
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

        _consoleOutput.ErrorLine($"Error: Unhandled bridge response command '{string.Join('/', commands)}' => '{message}' in {nameof(HandleBridgeResponseDeviceMessage)}.");
    }

    private void HandleBridgeResponsePermitJoinMessage(IReadOnlyList<string> additionalCommands, GenericMqttResponse parsedMessage)
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

    private void HandleBridgeResponseDeviceMessage(IReadOnlyList<string> commands, GenericMqttResponse parsedMessage)
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

        _consoleOutput.ErrorLine($"Error: Unhandled bridge device-response command '{string.Join('/', commands)}' in {nameof(HandleBridgeResponseDeviceMessage)}.");
    }

    private void HandleBridgeIgnoredMessage(string command, string message)
    {
        if(command == "logging")
            return; // Ignore without logging

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
                device.SendToBus = SendDataToDevice;
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

    private async Task SendDataToDevice(string path, Dictionary<string, string> data)
    {
        var message = JsonConvert.SerializeObject(data);
        _consoleOutput.InfoLine($"Z2M: Sending message to {path}: {message}");
        await _connection.Publish($"{_baseTopic}/{path}", message);
    }

    public async Task RequestOpenNetwork(int openNetworkTimeSeconds)
    {
        var data = new Dictionary<string, string> {
            ["value"] = "true",
            ["time"] = $"{openNetworkTimeSeconds}"
        };
        await SendDataToDevice("bridge/request/permit_join", data);
    }

    public async Task Connect() => await _connection.ConnectAsync();
}
