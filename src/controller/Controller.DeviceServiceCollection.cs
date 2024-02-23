using System.Reflection;

namespace LightAssistant.Controller;

internal partial class Controller
{
    private abstract class DeviceServiceCollection
    {
        internal IEnumerable<InternalEvent> ProcessExternalEvent(IReadOnlyDictionary<string, string> data)
        {
            foreach(var service in EnumerateServices()) {
                foreach(var ev in service.ProcessExternalEvent(data))
                    yield return ev;
            }
        }

        internal void ProcessInternalEvent(InternalEvent ev)
        {
            foreach(var service in EnumerateServices())
                service.ProcessInternalEvent(ev);
        }

        private IEnumerable<DeviceService> EnumerateServices() => GetType()
                .GetProperties(BindingFlags.NonPublic | BindingFlags.Instance)
                .Select(p => p.GetValue(this))
                .OfType<DeviceService>();
    }

    private class EmptyDeviceServiceCollection : DeviceServiceCollection { }
}
