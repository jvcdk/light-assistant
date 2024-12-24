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
                double stepSizeToDegrees, IConsoleOutput consoleOutput) : base(path, device, consoleOutput)
            {
                Button = new PushService("Push", device, consoleOutput) { Push = actionPush};
                Normal = new RotateService("Rotate normal", device, consoleOutput) { RotateLeft = actionNormalRotateLeft, RotateRight = actionNormalRotateRight, StepSizeToDegrees = stepSizeToDegrees };
                Pushed = new RotateService("Rotate pushed", device, consoleOutput) { RotateLeft = actionPushedRotateLeft, RotateRight = actionPushedRotateRight, StepSizeToDegrees = stepSizeToDegrees };
            }

            public PushService Button { get; private set; }
            public RotateService Normal { get; private set; }
            public RotateService Pushed { get; private set; }
        }
    }
}
