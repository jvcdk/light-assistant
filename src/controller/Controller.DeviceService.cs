
using System.Reflection;
using LightAssistant.Interfaces;

namespace LightAssistant.Controller;

internal partial class Controller
{
    private abstract class DeviceService(string name)
    {
        private const string KeywordAction = "action";

        internal string Name { get; private set; } = name;

        internal virtual IEnumerable<InternalEvent> ProcessExternalEvent(IDevice sourceDevice, IReadOnlyDictionary<string, string> data)
        {
            foreach(var service in EnumerateServices()) {
                foreach(var ev in service.ProcessExternalEvent(sourceDevice, data))
                    yield return ev;
            }
        }

        internal virtual void ProcessInternalEvent(InternalEvent ev, string targetFunctionality)
        {
            var eligibleServices = EnumerateServices()
                .Where(service => service.ConsumedEvents.Any(consumable =>
                    consumable.EventType == ev.GetType() &&
                    consumable.TargetName == targetFunctionality
                ));

            foreach(var service in eligibleServices)
                service.ProcessInternalEvent(ev, targetFunctionality);
        }

        private IEnumerable<DeviceService> EnumerateServices() => GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => p.GetValue(this))
                .OfType<DeviceService>();

        internal virtual IEnumerable<InternalEventSink> ConsumedEvents {
            get => EnumerateServices().SelectMany(service => service.ConsumedEvents);
        }

        internal virtual IEnumerable<InternalEventSource> ProvidedEvents { 
            get => EnumerateServices().SelectMany(service => service.ProvidedEvents);
        }


        internal class DimmableLightService() : DeviceService("")
        {
            internal override IEnumerable<InternalEventSink> ConsumedEvents => [
                new InternalEventSink(typeof(InternalEvent_Push), "ToggleOnOff"),
                new InternalEventSink(typeof(InternalEvent_Rotate), "Dim"),
                new InternalEventSink(typeof(InternalEvent_Rotate), "Fade")
            ];
        }

        internal class PushService(string path) : DeviceService(path)
        {
            public string Push { get; set; } = string.Empty;

            internal override IEnumerable<InternalEventSource> ProvidedEvents => [
                new InternalEventSource(typeof(InternalEvent_Push), Name)
            ];

            internal override IEnumerable<InternalEvent> ProcessExternalEvent(IDevice sourceDevice, IReadOnlyDictionary<string, string> data)
            {
                if(string.IsNullOrWhiteSpace(Push))
                    yield break;

                if(data.TryGetValue(KeywordAction, out var value) && value == Push)
                    yield return new InternalEvent_Push(sourceDevice.Address, Name);
            }
        }

        internal class RotateService(string name) : DeviceService(name)
        {
            public string RotateRight { get; set; } = string.Empty;
            public string RotateLeft { get; set; } = string.Empty;

            internal override IEnumerable<InternalEventSource> ProvidedEvents => [
                new InternalEventSource(typeof(InternalEvent_Rotate), Name)
            ];
        }

        internal class SmartKnobService: DeviceService
        {
            internal SmartKnobService(string path, string actionPush, string actionNormalRotateLeft, string actionNormalRotateRight, string actionPushedRotateLeft, string actionPushedRotateRight) : base(path)
            {
                Button = new PushService("Push") { Push = actionPush};
                Normal = new RotateService("Rotate normal") { RotateLeft = actionNormalRotateLeft, RotateRight = actionNormalRotateRight };
                Pushed = new RotateService("Rotate pushed") { RotateLeft = actionPushedRotateLeft, RotateRight = actionPushedRotateRight };
            }

            public PushService Button { get; private set; }
            public RotateService Normal { get; private set; }
            public RotateService Pushed { get; private set; }
        }

        internal class AutoModeChangeService() : DeviceService("")
        {
            public string ModeField { get; set; } = string.Empty;
            public string FromMode { get; set; } = string.Empty;
            public string ModeChangeCommand { get; set; } = string.Empty;
        }
    }
}
