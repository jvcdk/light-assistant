
namespace LightAssistant.Interfaces;

internal interface IUserInterface
{
    IController? AppController { set; }

    Task DeviceListUpdated();
    Task DeviceStateUpdated(string address, Dictionary<string, string> deviceStatus);
    void NetworkOpenStatusChanged(bool status, int time);
    Task DeviceDataUpdated(IDevice device);
    Task Run();
}
