
namespace LightAssistant.Interfaces;

internal interface IUserInterface
{
    IController? AppController { set; }

    Task DeviceListUpdated();
    Task DeviceStateUpdated(string address, IDeviceStatus deviceStatus);
    void NetworkOpenStatusChanged(bool status, int time);
    Task RoutingDataUpdated(IDevice device);
    Task Run();
}

internal interface IDeviceStatus
{
    int? LinkQuality { get; }
    int? Battery { get; }
    int? Brightness { get; }
    bool? State { get; }
}
