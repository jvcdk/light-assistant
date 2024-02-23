using LightAssistant.Interfaces;

namespace LightAssistant.Controller;

internal partial class Controller
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ProvidesEventAttribute : Attribute
    {
        public Type SrcType { get; }

        public ProvidesEventAttribute(Type srcType)
        {
            if(srcType.IsSubclassOf(typeof(InternalEvent)))
                throw new Exception("Invalid provider type. It should inherit from InternalEvent.");

            SrcType = srcType;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ConsumesEventAttribute : Attribute
    {
        public Type DstType { get; }

        public ConsumesEventAttribute(Type dstType)
        {
            if(dstType.IsSubclassOf(typeof(InternalEvent)))
                throw new Exception("Invalid consumer type. It should inherit from InternalEvent.");

            DstType = dstType;
        }
    }

    private partial class DeviceServiceMapping
    {
        private class ModelFactoryCollection : Dictionary<string, Func<DeviceServiceCollection>> { }

        private readonly IConsoleOutput _consoleOutput;
        private readonly Dictionary<string, ModelFactoryCollection> _factoryCollection = new();

        internal DeviceServiceMapping(IConsoleOutput consoleOutput)
        {
            _consoleOutput = consoleOutput;

            VendorTuya.Add(_factoryCollection);
            VendorSunricher.Add(_factoryCollection);
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
