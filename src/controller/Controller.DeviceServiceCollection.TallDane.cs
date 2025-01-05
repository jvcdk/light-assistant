
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

        private class PiPwm(IDevice device, IConsoleOutput consoleOutput) : DeviceServiceCollection(consoleOutput)
        {
            internal static DeviceServiceCollection Create(IDevice device, IConsoleOutput consoleOutput) => new PiPwm(device, consoleOutput);

            internal DeviceService.DimmableLightService Default { get; init; } = new(device, maxBrightness: (1<<15)-1, consoleOutput);

            protected override DeviceStatusConverter StatusConverter => new DeviceStatusConverter()
                .With("brightness", "Brightness", DeviceStatusConverter.Types.Identity)
                .With("state", "State", DeviceStatusConverter.Types.Bool);
        }
    }
}
