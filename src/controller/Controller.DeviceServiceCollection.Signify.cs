
using LightAssistant.Interfaces;

namespace LightAssistant.Controller;

internal partial class Controller
{
    private partial class DeviceServiceMapping
    {
        private static class VendorSignify
        {
            internal static void Add(Dictionary<string, ModelFactoryCollection> dst)
            {
                dst.Add("Signify Netherlands B.V.", new ModelFactoryCollection {
                    { "LWE007", SignifyE14Bluetooth.Create }
                });
            }
        }

        private class SignifyE14Bluetooth(IDevice device) : DeviceServiceCollection
        {
            internal static DeviceServiceCollection Create(IDevice device) => new SignifyE14Bluetooth(device);

            internal DeviceService.DimmableLightService Default { get; init; } = new(device, maxBrightness: 254);
        }
    }
}
