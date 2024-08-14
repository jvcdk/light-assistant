using LightAssistant.Interfaces;

namespace LightAssistant.Controller;

internal partial class Controller
{
    private abstract partial class DeviceService
    {
        internal class AutoModeChangeService(IDevice device) : DeviceService("", device)
        {
            public string ModeField { get; set; } = string.Empty;
            public string FromMode { get; set; } = string.Empty;
            public string ModeChangeCommand { get; set; } = string.Empty;
        }
    }
}
