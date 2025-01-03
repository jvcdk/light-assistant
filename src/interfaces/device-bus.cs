namespace LightAssistant.Interfaces;

internal interface IDeviceBus
{
    event Action<IDevice> DeviceDiscovered;
    event Action<IDevice> DeviceUpdated;
    event Action<IDevice, Dictionary<string, string>> DeviceAction;
    event Action<bool, int> NetworkOpenStatus;

    Task Connect();
    Task RequestOpenNetwork(int openNetworkTimeSeconds);
}

internal interface IDevice
{
    bool Equals(IDevice other);

    Task SendBrightnessTransition(int brightness, double transitionTime);
    Task SetName(string name);
    Task SendCommand(Dictionary<string, string> data);

    string Name { get; }
    string Address { get; }
    string Vendor { get; }
    string Model { get; }
    string Description { get; }
    bool BatteryPowered { get; }
}
