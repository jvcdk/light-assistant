using LightAssistant.Interfaces;
using static LightAssistant.Zigbee.Zigbee2MqttClient;

namespace LightAssistantOffline;

internal class MqttEmulatedClient : IDeviceBus, IDisposable
{
    public event Action<IDevice> DeviceDiscovered = delegate { };
    public event Action<IDevice, Dictionary<string, string>> DeviceAction = delegate { };
    public event Action<IDevice> DeviceUpdated  = delegate { };
    public event Action<bool, int> NetworkOpenStatus = delegate { };

    public MqttEmulatedClient()
    {
        Task.Run(() => ThreadMain());
    }

    private async void ThreadMain()
    {
        Thread.Sleep(1000);
        var smartKnob = new Device() {
            Name = "Tuya Smart Knob",
            Address = "0x4c5bb3fffe2e8acb",
            Vendor = "_TZ3000_qja6nq5z",
            Model = "TS004F",
            Definition = new DeviceDefinition() {
                Description = "Smart knob"
            },
            PowerSource = "Battery",
        };
        DeviceDiscovered(smartKnob);
        Thread.Sleep(100);
        var ledDriver = new Device() {
            Name = "Led Driver",
            Address = "0x94deb8fffe6aa0be",
            Vendor = "ENVILAR",
            Model = "HK-ZD-DIM-A",
            Definition = new DeviceDefinition() {
                Description = "Constant Current Zigbee LED dimmable driver"
            },
            PowerSource = "Mains (single phase)",
        };
        DeviceDiscovered(ledDriver);


        var rnd = new Random();
        while (!_disposed) {
            var sleepTime = rnd.Next(5000) + 1000;
            Thread.Sleep(sleepTime);

            var action = rnd.Next(2);
            switch (action) {
                case 0: {
                    var btnAction = rnd.Next(2) == 0 ? "color_temperature_step_" : "brightness_step_";
                    var direction = rnd.Next(2) == 0? "down" : "up";
                    var nSteps = rnd.Next(3) + 1;
                    for(var i = 0; i < nSteps; i++) {
                        var stepSize = ((rnd.Next(2) + 1) * 12).ToString();
                        var battery = rnd.Next(100).ToString();
                        var linkquality = rnd.Next(255).ToString();

                        DeviceAction(smartKnob,
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

                        var wait = rnd.Next(250);
                        await Task.Delay(wait);
                    break;
                }

                case 1: {
                    var battery = rnd.Next(100).ToString();
                    var linkquality = rnd.Next(255).ToString();
                    DeviceAction(smartKnob,
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

    private bool _disposed;

    public void Dispose()
    {
        _disposed = true;
    }

    public Task SetDeviceName(string address, string name)
    {
        throw new NotImplementedException();
    }

    public Task RequestOpenNetwork(int openNetworkTimeSeconds)
    {
        throw new NotImplementedException();
    }
}

