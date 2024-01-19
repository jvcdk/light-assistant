namespace LightAssistant.Interfaces;

internal interface IDeviceBus
{
    Task ConnectAsync();
}

internal interface IDeviceBusConnection
{
    event Action<IDevice> DeviceDiscovered;
    event Action<IDevice, Dictionary<string, string>> DeviceAction;
}

internal interface IDevice
{
    internal string Name { get; }
    internal string Address { get; }
    internal string Vendor { get; }
    internal string Model { get; }
    internal string Description { get; }
}
