
namespace LightAssistant.Interfaces
{
    internal interface IController
    {
        IReadOnlyList<IDevice> GetDeviceList();
        IDevice? TryGetDevice(string address);
        IEnumerable<IEventRoute> GetRoutingFor(IDevice device);
        Task SetDeviceOptions(string address, string name, IEnumerable<IEventRoute> routes, IDeviceScheduleEntry[] schedule);
        IRoutingOptions? GetRoutingOptionsFor(IDevice device);
        IReadOnlyList<IConsumableTrigger> GetConsumableTriggersFor(IDevice device);
        bool TryGetDeviceStatus(IDevice device, out IDeviceStatus? status);
        Task RequestOpenNetwork();
    }
}
