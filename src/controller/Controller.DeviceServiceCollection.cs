using System.Reflection;
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

        internal IEnumerable<TriggerInfo> ConsumedTriggers =>
            EnumerateServices().SelectMany(GetTriggers);

        private static IEnumerable<TriggerInfo> GetTriggers(DeviceService service)
        {
            return service.EnumerateMethodsWithAttribute<TriggerSink>()
                .Select(tuple => (tuple.method, tuple.attr, param: tuple.method.GetParameters()))
                .Where(tuple => tuple.param.Length == 1 && tuple.param[0].ParameterType.IsSubclassOf(typeof(TriggerEvent)))
                .Select(tuple => {
                    var paramInfo = tuple.param[0].ParameterType
                        .EnumeratePropertiesWithAttribute<ParamDescriptor>()
                        .Select(prop => new ParamInfo(prop.prop.Name, prop.attr!))
                        .ToList();
                    return (tuple.attr.Name, tuple.method, paramAttr:paramInfo);
                })
                .Select(tuple => new TriggerInfo(tuple.Name, tuple.method, tuple.paramAttr));
        }
    }

    private class EmptyDeviceServiceCollection : DeviceServiceCollection { }
}
