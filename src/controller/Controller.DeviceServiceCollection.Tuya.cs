
using LightAssistant.Interfaces;

namespace LightAssistant.Controller;

internal partial class Controller
{
    private partial class DeviceServiceMapping
    {
        private static class VendorTuya
        {
            internal static void Add(Dictionary<string, ModelFactoryCollection> dst)
            {
                dst.Add("_TZ3000_qja6nq5z", new ModelFactoryCollection {
                    { "TS004F", TuyaSmartKnob.Create }
                });
                dst.Add("_TZ3000_nkcobies", new ModelFactoryCollection {
                    { "TS011F", TuyaSmartPlug.Create }
                });
            }
        }

        private class TuyaSmartKnob : DeviceServiceCollection
        {
            public TuyaSmartKnob(IDevice device, IConsoleOutput consoleOutput) : base(consoleOutput)
            {
                AutoModeChange = new(device, consoleOutput) {
                    ModeField = "operation_mode",
                    FromMode = "event",
                    ToMode = "command"
                };

                Default = new(device,
                    path: "",
                    actionPush: "toggle", actionHold: "hue_move",
                    actionNormalRotateLeft: "brightness_step_down", actionNormalRotateRight: "brightness_step_up",
                    actionPushedRotateLeft: "color_temperature_step_down", actionPushedRotateRight: "color_temperature_step_up",
                    stepSizeToDegrees: 30.0 / 12.0,
                    consoleOutput);
            }

            internal static DeviceServiceCollection Create(IDevice device, IConsoleOutput consoleOutput) => new TuyaSmartKnob(device, consoleOutput);

            internal DeviceService.AutoModeChangeService AutoModeChange { get; }

            internal DeviceService.SmartKnobService Default { get; }

            protected override DeviceStatusConverter StatusConverter => new DeviceStatusConverter()
                .With("linkquality", "Link quality", DeviceStatusConverter.Types.Identity)
                .With("battery", "Battery", DeviceStatusConverter.Types.Percent)
                .With("voltage", "Voltage", DeviceStatusConverter.Types.MvToV);
        }

        private class TuyaSmartPlug : DeviceServiceCollection
        {
            public TuyaSmartPlug(IDevice device, IConsoleOutput consoleOutput) : base(consoleOutput)
            {
                Default = new(device, consoleOutput);
            }

            internal static DeviceServiceCollection Create(IDevice device, IConsoleOutput consoleOutput) => new TuyaSmartPlug(device, consoleOutput);

            internal DeviceService.SmartPlugService Default { get; }

            protected override DeviceStatusConverter StatusConverter => new DeviceStatusConverter()
                .With("linkquality", "Link quality", DeviceStatusConverter.Types.Identity)
                .With("state", "State", DeviceStatusConverter.Types.Bool)
                .With("voltage", "Voltage", (string value) => value + "V")
                .With("power", "Power", (string value) => value + "W")
                .With("current", "Current", (string value) => value + "A")
                .With("energy", "Energy", DeviceStatusConverter.Types.Identity);
        }
    }
}
