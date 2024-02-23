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

        private class SunricherCctLed : DeviceServiceCollection
        {
            internal static DeviceServiceCollection Create() => new SunricherCctLed();

            internal DeviceService.DimmableLightService Default { get; } = new();
        }
    }
}
