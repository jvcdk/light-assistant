
namespace LightAssistant.Interfaces
{
    internal interface IController
    {
        IReadOnlyList<IDevice> GetDeviceList();
    }
}