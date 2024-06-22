using LightAssistant.Interfaces;
using LightAssistant.Utils;

namespace LightAssistant.Controller;

internal partial class Controller
{
    private abstract class DeviceServiceCollection
    {
        internal IEnumerable<InternalEvent> ProcessExternalEvent(IDevice sourceDevice, IReadOnlyDictionary<string, string> data)
        {
            foreach(var service in EnumerateServices()) {
                foreach(var ev in service.ProcessExternalEvent(sourceDevice, data))
                    yield return ev;
            }
        }

        internal void ProcessInternalEvent(InternalEvent ev, string targetFunctionality)
        {
            foreach(var service in EnumerateServices())
                service.ProcessInternalEvent(ev, targetFunctionality);
        }

        private IEnumerable<DeviceService> EnumerateServices() => this.EnumeratePropertiesOfType<DeviceService>();

        internal IEnumerable<InternalEventSink> ConsumedEvents =>
            EnumerateServices().SelectMany(service => service.ConsumedEvents);

        internal IEnumerable<InternalEventSource> ProvidedEvents =>
            EnumerateServices().SelectMany(service => service.ProvidedEvents);
    }

    private class EmptyDeviceServiceCollection : DeviceServiceCollection { }
}
