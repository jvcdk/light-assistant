using LightAssistant.Interfaces;

namespace LightAssistant.Controller;

internal partial class Controller
{
    private abstract partial class DeviceService
    {
        internal class SmartKnobService: DeviceService
        {
            internal SmartKnobService(IDevice device, string path, string actionPush, 
                string actionNormalRotateLeft, string actionNormalRotateRight,
                string actionPushedRotateLeft, string actionPushedRotateRight,
                double unitStepSize) : base(path, device)
            {
                Button = new PushService("Push", device) { Push = actionPush};
                Normal = new RotateService("Rotate normal", device) { RotateLeft = actionNormalRotateLeft, RotateRight = actionNormalRotateRight, UnitStepSize = unitStepSize };
                Pushed = new RotateService("Rotate pushed", device) { RotateLeft = actionPushedRotateLeft, RotateRight = actionPushedRotateRight, UnitStepSize = unitStepSize };
            }

            public PushService Button { get; private set; }
            public RotateService Normal { get; private set; }
            public RotateService Pushed { get; private set; }
        }
    }
}
