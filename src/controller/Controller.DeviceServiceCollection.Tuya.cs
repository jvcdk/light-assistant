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
            internal static DeviceServiceCollection Create() => new TuyaSmartKnob();

            internal DeviceService.AutoModeChangeService AutoModeChange { get; } = new() {
                ModeField = "operation_mode",
                FromMode = "event",
                ModeChangeCommand = "some command"
            };

            internal DeviceService.SmartKnobService Default { get; } = new(path: "",
                actionPush: "toggle",
                actionNormalRotateLeft: "brightness_step_down", actionNormalRotateRight: "brightness_step_up",
                actionPushedRotateLeft: "color_temperature_step_down", actionPushedRotateRight: "color_temperature_step_up");
        }
    }
}
