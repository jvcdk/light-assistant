
using LightAssistant.Interfaces;

namespace LightAssistant.Controller;

internal partial class Controller
{
    private partial class DeviceServiceMapping
    {
        private static class VendorEWeLink
        {
            internal static void Add(Dictionary<string, ModelFactoryCollection> dst)
            {
                dst.Add("eWeLink", new ModelFactoryCollection {
                    { "WB01", EWeLink_WB01.Create }
                });
            }
        }

        private class EWeLink_WB01(IDevice device, IConsoleOutput consoleOutput) : DeviceServiceCollection(consoleOutput)
        {
            internal static DeviceServiceCollection Create(IDevice device, IConsoleOutput consoleOutput) => new EWeLink_WB01(device, consoleOutput);

            internal DeviceService.SingleButtonService Default { get; init; } = new(device, "", "single", "double", "long", consoleOutput);
        }
    }
}
