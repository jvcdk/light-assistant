using System.Diagnostics;
using LightAssistant;
using LightAssistant.Interfaces;
using static LightAssistant.Clients.Zigbee2MqttClient;

namespace LightAssistantOffline;

internal class MqttEmulatedClient : IDeviceBus, IDisposable
{
    public event Action<IDevice> DeviceDiscovered = delegate { };
    public event Action<IDevice, Dictionary<string, string>> DeviceAction = delegate { };
    public event Action<IDevice> DeviceUpdated  = delegate { };
    public event Action<bool, int> NetworkOpenStatus = delegate { };

    private readonly ConsoleOutput _consoleOutput;

    private Device? _smartKnob;
    private Device? _ledDriver;

    public MqttEmulatedClient(ConsoleOutput consoleOutput)
    {
        _consoleOutput = consoleOutput;
    }

    private void ThreadMain()
    {
        InitializeDevices();
        Debug.Assert(_smartKnob != null);
        Debug.Assert(_ledDriver != null);

        var rnd = new Random();
        while (!_disposed) {
            var sleepTime = rnd.Next(5000) + 1000;
            Thread.Sleep(sleepTime);

            var action = rnd.Next(2);
            switch (action) {
                case 0: {
                        var btnAction = rnd.Next(2) == 0 ? "color_temperature_step_" : "brightness_step_";
                        var direction = rnd.Next(2) == 0 ? "down" : "up";
                        var nSteps = rnd.Next(3) + 1;
                        for (var i = 0; i < nSteps; i++) {
                            var stepSize = ((rnd.Next(2) + 1) * 12).ToString();
                            var battery = rnd.Next(100).ToString();
                            var linkquality = rnd.Next(255).ToString();

                            DeviceAction(_smartKnob,
                                new Dictionary<string, string>() {
                                {"action", btnAction + direction},
                                {"action_step_size", stepSize},
                                {"action_transition_time", "0.01"},
                                {"battery", battery},
                                {"linkquality", linkquality},
                                {"operation_mode", "command"},
                                {"voltage", "3000"}
                                });
                        }
                        break;
                    }

                case 1: {
                        var battery = rnd.Next(100).ToString();
                        var linkquality = rnd.Next(255).ToString();
                        DeviceAction(_smartKnob,
                            new Dictionary<string, string>() {
                            {"action", "toggle"},
                            {"action_transition_time", "0.01"},
                            {"battery", battery},
                            {"linkquality", linkquality},
                            {"operation_mode", "command"},
                            {"voltage", "3000"}
                            });
                        break;
                    }
            }
        }
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
            return Task.CompletedTask;
        };
        DeviceDiscovered(_ledDriver);
    }

    private bool _disposed;

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

