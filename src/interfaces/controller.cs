namespace LightAssistant.Interfaces
{
    internal interface IController
    {
        IReadOnlyList<IDevice> GetDeviceList();
        IEnumerable<IEventRoute> GetRoutingFor(IDevice device);
        IRoutingOptions? GetRoutingOptionsFor(IDevice device);
        bool TryGetDeviceStatus(IDevice device, out IDeviceStatus? status);
    }
}
