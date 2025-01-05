
using LightAssistant.Interfaces;

namespace LightAssistant.Controller;

internal partial class Controller
{
    private partial class DeviceServiceMapping
    {
        private static class VendorSunricher
        {
            internal static void Add(Dictionary<string, ModelFactoryCollection> dst)
            {
                dst.Add("ENVILAR", new ModelFactoryCollection {
                    { "HK-ZD-DIM-A", SunricherCctLed.Create }
                });
            }
        }

        private class SunricherCctLed(IDevice device, IConsoleOutput consoleOutput) : DeviceServiceCollection(consoleOutput)
        {
            internal static DeviceServiceCollection Create(IDevice device, IConsoleOutput consoleOutput) => new SunricherCctLed(device, consoleOutput);

            internal DeviceService.DimmableLightService Default { get; init; } = new(device, maxBrightness: 254, consoleOutput);

            protected override DeviceStatusConverter StatusConverter => new DeviceStatusConverter()
                .With("linkquality", "Link quality", DeviceStatusConverter.Types.Identity)
                .With("brightness", "Brightness", DeviceStatusConverter.Types.Identity)
                .With("state", "State", DeviceStatusConverter.Types.Bool);
        }
    }
}
