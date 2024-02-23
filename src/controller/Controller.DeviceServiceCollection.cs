namespace LightAssistant.Controller;

internal partial class Controller
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DeviceServiceAttribute(string name) : Attribute
    {
        public string Name { get; } = name;
    }

    private abstract class DeviceServiceCollection
    {

    }

    private class EmptyDeviceServiceCollection : DeviceServiceCollection { }
}

