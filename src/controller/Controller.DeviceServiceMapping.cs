using LightAssistant.Interfaces;

namespace LightAssistant.Controller;

internal partial class Controller
{
    private partial class DeviceServiceMapping
    {
        private class ModelFactoryCollection : Dictionary<string, Func<IDevice, DeviceServiceCollection>> { }

        private readonly IConsoleOutput _consoleOutput;
        private readonly Dictionary<string, ModelFactoryCollection> _factoryCollection = [];

        internal DeviceServiceMapping(IConsoleOutput consoleOutput)
        {
            _consoleOutput = consoleOutput;

            VendorTuya.Add(_factoryCollection);
            VendorSunricher.Add(_factoryCollection);
            VendorSignify.Add(_factoryCollection);
        }

        internal DeviceServiceCollection GetServicesFor(IDevice device)
        {
            if(_factoryCollection.TryGetValue(device.Vendor, out var modelFactoryCollection)) {
                if(modelFactoryCollection.TryGetValue(device.Model, out var factory))
                    return factory.Invoke(device);
            }

            _consoleOutput.ErrorLine($"No services found for device '{device.Name}' (Vendor: {device.Vendor}, Model: {device.Model}).");
            return new EmptyDeviceServiceCollection();
        }
    }
}
