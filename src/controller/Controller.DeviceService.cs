using LightAssistant.Interfaces;
using LightAssistant.Utils;

namespace LightAssistant.Controller;

internal partial class Controller
{
    private abstract partial class DeviceService
    {
        private const string KeywordAction = "action";
        private const string KeywordActionStepSize = "action_step_size";

        internal IConsoleOutput ConsoleOutput { get; }
        internal string Name { get; private set; }

        protected readonly IDevice Device;

        protected DeviceService(string name, IDevice device, IConsoleOutput consoleOutput)
        {
            Name = name;
            Device = device;
            ConsoleOutput = consoleOutput;
        }

        internal virtual IEnumerable<InternalEvent> ProcessExternalEvent(IDevice sourceDevice, IReadOnlyDictionary<string, string> data)
        {
            foreach(var service in EnumerateServices()) {
                foreach(var ev in service.ProcessExternalEvent(sourceDevice, data))
                    yield return ev;
            }
        }

        internal virtual void ProcessInternalEvent(InternalEvent ev, string targetFunctionality)
        {
            foreach(var evDst in ConsumedEvents)
                if(evDst.TargetName == targetFunctionality)
                    evDst.Handler(ev);
        }

        private IEnumerable<DeviceService> EnumerateServices() => this.EnumeratePropertiesOfType<DeviceService>();

        internal virtual IEnumerable<InternalEventSink> ConsumedEvents {
            get => EnumerateServices().SelectMany(service => service.ConsumedEvents);
        }

        internal virtual IEnumerable<InternalEventSource> ProvidedEvents =>
            EnumerateServices().SelectMany(service => service.ProvidedEvents);
    }
}
