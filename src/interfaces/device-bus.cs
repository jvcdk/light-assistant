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
    string Name { get; }
    string Address { get; }
    string Vendor { get; }
    string Model { get; }
    string Description { get; }
}
