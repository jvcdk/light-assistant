
using System.Reflection;
using LightAssistant.Interfaces;

namespace LightAssistant.Controller;

internal partial class Controller
{
    private abstract class DeviceService
    {
        private const string KeywordAction = "action";

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
                    consumable.Event == ev.GetType() &&
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

        internal virtual IEnumerable<Type> ProvidedEvents { 
            get => EnumerateServices().SelectMany(service => service.ProvidedEvents);
        }

        internal class DimmableLightService : DeviceService
        {
            private readonly List<InternalEventSink> _consumedEvents = new() {
                new InternalEventSink(typeof(InternalEvent_ButtonPush), "ToggleOnOff"),
                new InternalEventSink(typeof(InternalEvent_Rotate), "Dim"),
                new InternalEventSink(typeof(InternalEvent_Rotate), "Fade")
            };

            internal override IEnumerable<InternalEventSink> ConsumedEvents => _consumedEvents;
        }

        internal class PushService : DeviceService
        {
            public string Push { get; set; } = string.Empty;

            internal override IEnumerable<Type> ProvidedEvents => [
                typeof(InternalEvent_ButtonPush)
            ];

            internal override IEnumerable<InternalEvent> ProcessExternalEvent(IDevice sourceDevice, IReadOnlyDictionary<string, string> data)
            {
                if(string.IsNullOrWhiteSpace(Push))
                    yield break;

                if(data.TryGetValue(KeywordAction, out var value) && value == Push)
                    yield return new InternalEvent_ButtonPush(sourceDevice.Address);
            }
        }

        internal class RotateService : DeviceService
        {
            public string RotateRight { get; set; } = string.Empty;
            public string RotateLeft { get; set; } = string.Empty;

            internal override IEnumerable<Type> ProvidedEvents => [
                typeof(InternalEvent_Rotate)
            ];
        }

        internal class SmartKnobService(PushService push, RotateService rotateNormal, RotateService rotatePushed) : DeviceService
        {
            public PushService Push { get; set; } = push;
            public RotateService RotateNormal { get; set; } = rotateNormal;
            public RotateService RotatePushed { get; set; } = rotatePushed;
        }

        internal class AutoModeChangeService : DeviceService
        {
            public string ModeField { get; set; } = string.Empty;
            public string FromMode { get; set; } = string.Empty;
            public string ModeChangeCommand { get; set; } = string.Empty;
        }
    }
}
