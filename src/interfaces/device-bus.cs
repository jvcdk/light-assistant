namespace LightAssistant.Interfaces;

internal interface IDeviceBus
{
    Task ConnectAsync();
    event EventHandler<IDevice> DeviceDiscovered;
}

internal interface IDevice
{
    internal string Address { get; }
    internal string Vendor { get; }
    internal string Model { get; }
    internal string Description { get; }
}
