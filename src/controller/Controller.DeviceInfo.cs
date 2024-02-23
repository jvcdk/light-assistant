namespace LightAssistant.Controller;

internal partial class Controller
{
    private class DeviceInfo
    {
        public DeviceStatus Status = new();
        public DeviceServiceCollection Services = new EmptyDeviceServiceCollection();
    }
}
