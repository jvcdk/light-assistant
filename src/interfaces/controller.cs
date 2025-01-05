
namespace LightAssistant.Interfaces
{
    internal interface IController
    {
        IReadOnlyList<IDevice> GetDeviceList();
        IDevice? TryGetDevice(string address);
        IEnumerable<IEventRoute> GetRoutingFor(IDevice device);
        IEnumerable<IDeviceScheduleEntry> GetScheduleFor(IDevice device);
        Task SetDeviceOptions(string address, string name, IEnumerable<IEventRoute> routes, IDeviceScheduleEntry[] schedule, IServiceOptionValue[] serviceOptionValues);
        IRoutingOptions? GetRoutingOptionsFor(IDevice device);
        IReadOnlyList<IConsumableAction> GetConsumableActionsFor(IDevice device);
        IReadOnlyList<IServiceOption> GetServiceOptionsFor(IDevice device);
        bool TryGetDeviceStatus(IDevice device, out Dictionary<string, string>? status);
        Task PreviewDeviceOption(string address, string value, PreviewMode previewMode);
        Task RequestOpenNetwork();
    }
}
