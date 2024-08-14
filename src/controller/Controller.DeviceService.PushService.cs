using LightAssistant.Interfaces;

namespace LightAssistant.Controller;

internal partial class Controller
{
    private abstract partial class DeviceService
    {
        internal class PushService(string path, IDevice device) : DeviceService(path, device)
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
    }
}
