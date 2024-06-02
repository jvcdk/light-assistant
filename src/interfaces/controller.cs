namespace LightAssistant.Interfaces
{
    internal interface IController
    {
        IReadOnlyList<IDevice> GetDeviceList();
        IDevice? TryGetDevice(string address);
        IEnumerable<IEventRoute> GetRoutingFor(IDevice device);
        Task SetRoutingFor(IDevice device, IEnumerable<IEventRoute> routes);
        IRoutingOptions? GetRoutingOptionsFor(IDevice device);
        bool TryGetDeviceStatus(IDevice device, out IDeviceStatus? status);
    }
}
