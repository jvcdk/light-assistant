
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
            public TuyaSmartKnob(IDevice device)
            {
                AutoModeChange = new(device) {
                    ModeField = "operation_mode",
                    FromMode = "event",
                    ModeChangeCommand = "some command"
                };

                Default = new(device,
                    path: "",
                    actionPush: "toggle",
                    actionNormalRotateLeft: "brightness_step_down", actionNormalRotateRight: "brightness_step_up",
                    actionPushedRotateLeft: "color_temperature_step_down", actionPushedRotateRight: "color_temperature_step_up");
            }

            internal static DeviceServiceCollection Create(IDevice device) => new TuyaSmartKnob(device);

            internal DeviceService.AutoModeChangeService AutoModeChange { get; }

            internal DeviceService.SmartKnobService Default { get; }
        }
    }
}
