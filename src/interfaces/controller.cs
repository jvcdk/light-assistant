namespace LightAssistant.Interfaces
{
    internal interface IController
    {
        IReadOnlyList<IDevice> GetDeviceList();
        IDevice? TryGetDevice(string address);
        IEnumerable<IEventRoute> GetRoutingFor(IDevice device);
        Task SetRoutingFor(IDevice device, IEnumerable<IEventRoute> routes);
        Task SetDeviceName(IDevice device, string name);
        IRoutingOptions? GetRoutingOptionsFor(IDevice device);
        bool TryGetDeviceStatus(IDevice device, out IDeviceStatus? status);
    }
}
