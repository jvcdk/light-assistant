using System.Diagnostics;
using LightAssistant;
using LightAssistant.Interfaces;
using static LightAssistant.Clients.Zigbee2MqttClient;

namespace LightAssistantOffline;

internal class MqttEmulatedClient(ConsoleOutput consoleOutput) : IDeviceBus, IDisposable
{
    public event Action<IDevice> DeviceDiscovered = delegate { };
    public event Action<IDevice, Dictionary<string, string>> DeviceAction = delegate { };
    public event Action<IDevice> DeviceUpdated  = delegate { };
    public event Action<bool, int> NetworkOpenStatus = delegate { };

    private readonly ConsoleOutput _consoleOutput = consoleOutput;
    private readonly Random _rnd = new();

    private Device? _smartKnob;
    private Device? _ledDriver;

    private void ThreadMain()
    {
        InitializeDevices();
        Debug.Assert(_smartKnob != null);
        Debug.Assert(_ledDriver != null);

        while (!_disposed) {
            SendSmartKnobCommand("toggle");
            Thread.Sleep(1500);
            if(_disposed)
                return;

            SendSmartKnobCommand("toggle");
            Thread.Sleep(1500);
            if(_disposed)
                return;

            for (var i = 0; i < 10; i++) {
                SendSmartKnobCommand("brightness_step_up", "12");
                Thread.Sleep(350);
                if(_disposed)
                    return;
            }
            Thread.Sleep(1500);
            if(_disposed)
                return;

            SendSmartKnobCommand("color_temperature_step_down", "12");
            Thread.Sleep(10000);
            if(_disposed)
                return;
        }
    }

    private void SendSmartKnobCommand(string action, string? stepSize = null)
    {
        Debug.Assert(_smartKnob != null);

        var battery = _rnd.Next(100).ToString();
        var linkquality = _rnd.Next(255).ToString();
        var voltage = _rnd.Next(3000).ToString();
        var @params = new Dictionary<string, string>() {
            {"action", action},
            {"action_transition_time", "0.01"},
            {"battery", battery},
            {"linkquality", linkquality},
            {"operation_mode", "command"},
            {"voltage", voltage}
        };
        if(stepSize != null)
            @params.Add("action_step_size", stepSize);
        
        DeviceAction(_smartKnob, @params);
    }

    private void InitializeDevices()
    {
        _smartKnob = new Device() {
            Name = "Tuya Smart Knob",
            Address = "0x4c5bb3fffe2e8acb",
            Vendor = "_TZ3000_qja6nq5z",
            Model = "TS004F",
            Definition = new DeviceDefinition() {
                Description = "Smart knob"
            },
            PowerSource = "Battery",
        };
        _smartKnob.SendToBus += (deviceId, data) => {
            _consoleOutput.MessageLine($"TX to Tuya Smart Knob sent data: {string.Join(", ", data.Select(kv => $"{kv.Key}={kv.Value}"))}");
            return Task.CompletedTask;
        };
        DeviceDiscovered(_smartKnob);

        Thread.Sleep(100);

        _ledDriver = new Device() {
            Name = "Led Driver",
            Address = "0x94deb8fffe6aa0be",
            Vendor = "ENVILAR",
            Model = "HK-ZD-DIM-A",
            Definition = new DeviceDefinition() {
                Description = "Constant Current Zigbee LED dimmable driver"
            },
            PowerSource = "Mains (single phase)",
        };
        _ledDriver.SendToBus += (deviceId, data) => {
            _consoleOutput.MessageLine($"TX to Led Driver sent data: {string.Join(", ", data.Select(kv => $"{kv.Key}={kv.Value}"))}");
            SendLedDriverReply(data);
            return Task.CompletedTask;
        };
        DeviceDiscovered(_ledDriver);
    }

    private void SendLedDriverReply(Dictionary<string, string> data)
    {
        if(_ledDriver == null)
            return;
        if(!data.TryGetValue("brightness", out var brightness))
            brightness = "0";
        var @params = new Dictionary<string, string> {
                {"brightness", brightness},
                {"linkquality", _rnd.Next(255).ToString()},
                {"state", data["state"]}
            };
        DeviceAction(_ledDriver, @params);
    }

    private volatile bool _disposed;

    public void Dispose()
    {
        _disposed = true;
    }

    public Task RequestOpenNetwork(int openNetworkTimeSeconds)
    {
        throw new NotImplementedException();
    }

    public Task Connect()
    {
        var thread = new Thread(ThreadMain);
        thread.Name = "MqttEmulatedClient";
        thread.Start();
        return Task.CompletedTask;
    }
}

