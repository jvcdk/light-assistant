using LightAssistant.Interfaces;

namespace LightAssistant.Controller;

internal partial class Controller
{
    private partial class DeviceServiceMapping
    {
        private class ModelFactoryCollection : Dictionary<string, Func<IDevice, IConsoleOutput, DeviceServiceCollection>> { }

        private readonly IConsoleOutput _consoleOutput;
        private readonly Dictionary<string, ModelFactoryCollection> _factoryCollection = [];

        internal DeviceServiceMapping(IConsoleOutput consoleOutput)
        {
            _consoleOutput = consoleOutput;

            VendorSignify.Add(_factoryCollection);
            VendorSunricher.Add(_factoryCollection);
            VendorTallDane.Add(_factoryCollection);
            VendorTuya.Add(_factoryCollection);
        }

        internal DeviceServiceCollection GetServicesFor(IDevice device, IReadOnlyList<IServiceOptionValue>? serviceOptionValues)
        {
            if(_factoryCollection.TryGetValue(device.Vendor, out var modelFactoryCollection)) {
                if(modelFactoryCollection.TryGetValue(device.Model, out var factory)) {
                    var result = factory.Invoke(device, _consoleOutput);

                    if(serviceOptionValues != null)
                        result.SetServiceOptionValues(serviceOptionValues);

                    return result;
                }
            }

            _consoleOutput.ErrorLine($"No services found for device '{device.Name}' (Vendor: {device.Vendor}, Model: {device.Model}).");
            return new EmptyDeviceServiceCollection();
        }
    }
}
