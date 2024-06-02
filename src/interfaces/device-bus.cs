namespace LightAssistant.Interfaces;

internal interface IDeviceBus
{
    event Action<IDevice> DeviceDiscovered;
    event Action<IDevice> DeviceUpdated;
    event Action<IDevice, Dictionary<string, string>> DeviceAction;
    Task SetDeviceName(string address, string name);
}

internal interface IDevice
{
    bool Equals(IDevice other);

    string Name { get; }
    string Address { get; }
    string Vendor { get; }
    string Model { get; }
    string Description { get; }
    bool BatteryPowered { get; }
}
