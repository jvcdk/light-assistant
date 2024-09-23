
using LightAssistant.Interfaces;

namespace LightAssistant.Controller;

internal partial class Controller
{
    private partial class DeviceServiceMapping
    {
        private static class VendorTallDane
        {
            internal static void Add(Dictionary<string, ModelFactoryCollection> dst)
            {
                dst.Add("TallDane", new ModelFactoryCollection {
                    { "Pi5", PiPwm.Create }
                });
            }
        }

        private class PiPwm(IDevice device) : DeviceServiceCollection
        {
            internal static DeviceServiceCollection Create(IDevice device) => new PiPwm(device);

            internal DeviceService.DimmableLightService Default { get; init; } = new(device, maxBrightness: (1<<15)-1);
        }
    }
}
