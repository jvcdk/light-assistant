namespace LightAssistant;

internal partial class Controller
{
    private abstract class DeviceService
    {
        internal class DimmableLightService : DeviceService
        {

        }

        internal class PushService : DeviceService
        {
            public string Push { get; set; } = string.Empty;
        }

        internal class RotateService : DeviceService
        {
            public string RotateRight { get; set; } = string.Empty;
            public string RotateLeft { get; set; } = string.Empty;
        }

        internal class SmartKnobService(PushService push, RotateService rotateNormal, RotateService rotatePushed) : DeviceService
        {
            public PushService Push { get; set; } = push;
            public RotateService RotateNormal { get; set; } = rotateNormal;
            public RotateService RotatePushed { get; set; } = rotatePushed;
        }

        internal class AutoModeChangeService : DeviceService
        {
            public string ModeField { get; set; } = string.Empty;
            public string FromMode { get; set; } = string.Empty;
            public string ModeChangeCommand { get; set; } = string.Empty;
        }
    }
}
