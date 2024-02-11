namespace LightAssistant;

internal partial class Controller
{
    private class DeviceInfo
    {
        public DeviceStatus Status = new();
        public List<DeviceService> Services = [];
    }
}
