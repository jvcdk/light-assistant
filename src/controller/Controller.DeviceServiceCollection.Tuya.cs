
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
        }
    }
}
