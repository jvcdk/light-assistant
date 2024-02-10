
namespace LightAssistant.Interfaces
{
    internal interface IController
    {
        IReadOnlyList<IDevice> GetDeviceList();
        bool TryGetDeviceStatus(IDevice device, out IDeviceStatus? status);
    }
}