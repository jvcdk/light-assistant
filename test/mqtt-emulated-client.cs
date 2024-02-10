using LightAssistant.Interfaces;
using static LightAssistant.Zigbee.Zigbee2MqttClient;

namespace LightAssistantOffline;

internal class MqttEmulatedClient : IDeviceBusConnection, IDisposable
{
    public event Action<IDevice> DeviceDiscovered = delegate { };
    public event Action<IDevice, Dictionary<string, string>> DeviceAction = delegate { };

    public MqttEmulatedClient()
    {
        Task.Run(() => ThreadMain());
    }

    private void ThreadMain()
    {
        Thread.Sleep(1000);
        var smartKnob = new Device() {
            Name = "Tuya Smart Knob",
            Address = "0x4c5bb3fffe2e8acb",
            Vendor = "_TZ3000_qja6nq5z",
            Model = "TS004F",
            Definition = new DeviceDefinition() {
                Description = "Smart knob"
            }
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
            }
        };
        DeviceDiscovered(ledDriver);


        var rnd = new Random();
        while (!_disposed) {
            var sleepTime = rnd.Next(900) + 100;
            Thread.Sleep(sleepTime);

            var action = rnd.Next(3);
            switch (action) {
                case 0: {
                    var brightness = rnd.Next(10) * 10;
                    var state = brightness == 0 ? "OFF" : "ON";
                    DeviceAction(ledDriver,
                        new Dictionary<string, string>() {
                            {"brightness", brightness.ToString()},
                            {"level_config", "\"on_level\":\"previous\"}"},
                            {"state", state}
                        });
                    break;
                }
                case 1: {
                    var stepSize = (rnd.Next(5) * 12).ToString();
                    var btnAction = rnd.Next(2) == 0 ? "color_temperature_step_" : "brightness_step_";
                    var direction = rnd.Next(2) == 0? "down" : "up";
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
                    break;
                }

                case 2: {
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
}

