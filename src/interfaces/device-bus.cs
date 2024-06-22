namespace LightAssistant.Interfaces;

internal interface IDeviceBus
{
    event Action<IDevice> DeviceDiscovered;
    event Action<IDevice> DeviceUpdated;
    event Action<IDevice, Dictionary<string, string>> DeviceAction;
    event Action<bool, int> NetworkOpenStatus;

    Task RequestOpenNetwork(int openNetworkTimeSeconds);
    Task SetDeviceName(string address, string name);
}

internal interface IDevice
{
    bool Equals(IDevice other);

    void SendBrightnessTransition(int brightness, int defaultTransitionTime);

    string Name { get; }
    string Address { get; }
    string Vendor { get; }
    string Model { get; }
    string Description { get; }
    bool BatteryPowered { get; }
}
