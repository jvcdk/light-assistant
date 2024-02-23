namespace LightAssistant.Controller;

internal partial class Controller
{
    private partial class DeviceMapping
    {
        private class ModelFactoryCollection : Dictionary<string, Func<DeviceServiceCollection>> { }

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

            [DeviceService("AutoModeChange")]
            internal DeviceService.AutoModeChangeService AutoModeChange { get; } = new() {
                ModeField = "operation_mode",
                FromMode = "event",
                ModeChangeCommand = "some command"
            };

            [DeviceService("Default")]
            internal DeviceService.SmartKnobService Default { get; } = new (
                    push: new DeviceService.PushService { Push = "toggle"},
                    rotateNormal: new DeviceService.RotateService { RotateLeft = "brightness_step_down", RotateRight = "brightness_step_up" },
                    rotatePushed: new DeviceService.RotateService { RotateLeft = "color_temperature_step_down", RotateRight = "color_temperature_step_up" }
                );
        }
    }
}
