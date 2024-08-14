using LightAssistant.Interfaces;

namespace LightAssistant.Controller;

internal partial class Controller
{
    private abstract partial class DeviceService
    {
        internal class RotateService(string name, IDevice device) : DeviceService(name, device)
        {
            public string RotateRight { get; init; } = string.Empty;
            public string RotateLeft { get; init; } = string.Empty;
            public double UnitStepSize { get; init; }

            internal override IEnumerable<InternalEventSource> ProvidedEvents => [
                new InternalEventSource(typeof(InternalEvent_Rotate), Name)
            ];

            internal override IEnumerable<InternalEvent> ProcessExternalEvent(IDevice sourceDevice, IReadOnlyDictionary<string, string> data)
            {
                if(!data.TryGetValue(KeywordActionStepSize, out var stepSizeStr))
                    yield break;;

                if(!int.TryParse(stepSizeStr, out var stepSizeInt))
                    yield break;;

                var stepSize = stepSizeInt / UnitStepSize;
                if(Match(data, RotateRight))
                    yield return new InternalEvent_Rotate(sourceDevice.Address, Name) {
                        StepSize = stepSize,
                        IsUp = true,
                    };
                else if(Match(data, RotateLeft))
                    yield return new InternalEvent_Rotate(sourceDevice.Address, Name) {
                        StepSize = stepSize,
                        IsUp = false,
                    };
            }

            private static bool Match(IReadOnlyDictionary<string, string> haystack, string needle)
            {
                return !string.IsNullOrWhiteSpace(needle) &&
                    haystack.TryGetValue(KeywordAction, out var value) &&
                    needle == value;
            }
        }
    }
}
