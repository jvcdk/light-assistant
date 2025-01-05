
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

        private class SignifyE14Bluetooth(IDevice device, IConsoleOutput consoleOutput) : DeviceServiceCollection(consoleOutput)
        {
            internal static DeviceServiceCollection Create(IDevice device, IConsoleOutput consoleOutput) => new SignifyE14Bluetooth(device, consoleOutput);

            internal DeviceService.DimmableLightService Default { get; init; } = new(device, maxBrightness: 254, consoleOutput);

            protected override DeviceStatusConverter StatusConverter => new DeviceStatusConverter()
                .With("linkquality", "Link quality", DeviceStatusConverter.Types.Identity)
                .With("brightness", "Brightness", DeviceStatusConverter.Types.Identity)
                .With("state", "State", DeviceStatusConverter.Types.Bool);
        }
    }
}
