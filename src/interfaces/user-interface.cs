
namespace LightAssistant.Interfaces
{
    internal interface IUserInterface
    {
        IController? AppController { set; }

        Task DeviceListUpdated();
        Task Run();
    }
}