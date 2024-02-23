using LightAssistant.Interfaces;

namespace LightAssistant.Controller;

internal partial class Controller
{
    private partial class DeviceMapping
    {
        private readonly IConsoleOutput _consoleOutput;
        private readonly Dictionary<string, ModelFactoryCollection> _factoryCollection = new();

        internal DeviceMapping(IConsoleOutput consoleOutput)
        {
            _consoleOutput = consoleOutput;

            VendorTuya.Add(_factoryCollection);
        }

        internal DeviceServiceCollection GetServicesFor(IDevice device)
        {
            if(_factoryCollection.TryGetValue(device.Vendor, out var modelFactoryCollection)) {
                if(modelFactoryCollection.TryGetValue(device.Model, out var factory))
                    return factory.Invoke();
            }

            _consoleOutput.ErrorLine($"No services found for device '{device.Name}' (Vendor: {device.Vendor}, Model: {device.Model}).");
            return new EmptyDeviceServiceCollection();
        }
    }
}
