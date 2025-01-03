using LightAssistant.Interfaces;

namespace LightAssistant.Controller;

internal partial class Controller
{
    private abstract partial class DeviceService
    {
        internal class AutoModeChangeService(IDevice device, IConsoleOutput consoleOutput) : DeviceService("", device, consoleOutput)
        {
            public string ModeField { get; set; } = string.Empty;
            public string FromMode { get; set; } = string.Empty;
            public string ToMode { get; set; } = string.Empty;

            internal override IEnumerable<InternalEvent> ProcessExternalEvent(IDevice sourceDevice, IReadOnlyDictionary<string, string> data)
            {
                if (string.IsNullOrWhiteSpace(ModeField) || string.IsNullOrWhiteSpace(FromMode) || string.IsNullOrWhiteSpace(ToMode))
                    yield break;

                if (!data.TryGetValue(ModeField, out var value) || value != FromMode)
                    yield break;

                var cmd = new Dictionary<string, string> {
                    [ModeField] = ToMode
                };
                Device.SendCommand(cmd);
            }

        }
    }
}
