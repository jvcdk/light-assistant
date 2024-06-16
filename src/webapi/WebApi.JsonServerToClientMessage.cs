using LightAssistant.Interfaces;
using Newtonsoft.Json;
using static System.Text.Encoding;

namespace LightAssistant.WebApi;

internal partial class WebApi
{
    public class JsonServerToClientMessage
    {
        internal byte[] Serialize() => UTF8.GetBytes(JsonConvert.SerializeObject(this));

        public IReadOnlyList<JsonDevice>? Devices { get; private set; }
        public JsonDeviceStatus? DeviceStatus { get; private set; }
        public JsonDeviceRouting? Routing { get; private set; }
        public JsonDeviceRoutingOptions? RoutingOptions { get; private set; }

        internal static JsonServerToClientMessage Empty() => new();

        internal JsonServerToClientMessage WithDeviceList(IReadOnlyList<IDevice> devices) {
            Devices = devices.Select(JsonDevice.FromIDevice).ToList();
            return this;
        }

        internal JsonServerToClientMessage WithDeviceStatus(string address, IDeviceStatus status) {
            DeviceStatus = new JsonDeviceStatus(address, status);
            return this;
        }

        internal JsonServerToClientMessage WithDeviceRouting(string address, IReadOnlyList<JsonDeviceRoute> routing) {
            Routing = new JsonDeviceRouting(address, routing);
            return this;
        }

        internal JsonServerToClientMessage WithDeviceRoutingOptions(string address, IReadOnlyList<JsonDeviceProvidedEvent> providedEvents, IReadOnlyList<JsonDeviceConsumableEvent> consumableEvents) {
            RoutingOptions = new JsonDeviceRoutingOptions(address, providedEvents, consumableEvents);
            return this;
        } 
    }

    /// <summary>
    /// Serializing an interface (i.e. IDevice) directly, does not produce json information for the interface,
    /// but for the implementing object. Hence we need to create a specific object for IDevice.
    /// 
    /// Must match IDevice in Device.tsx
    /// </summary>
    public class JsonDevice(IDevice source)
    {
        public string Name { get; } = source.Name;
        public string Address { get; } = source.Address;
        public string Vendor { get; } = source.Vendor;
        public string Model { get; } = source.Model;
        public string Description { get; } = source.Description;
        public bool BatteryPowered { get; } = source.BatteryPowered;

        public static JsonDevice FromIDevice(IDevice source) => new(source);
    }

    /// <summary>
    /// Must match IDeviceStatus in Device.tsx
    /// </summary>
    public class JsonDeviceStatus(string address, IDeviceStatus source)
    {
        public string Address { get; } = address;
        public int? LinkQuality { get; } = source.LinkQuality;
        public int? Battery { get; } = source.Battery;
        public int? Brightness { get; } = source.Brightness;
        public bool? State { get; } = source.State;
    }

    /// <summary>
    /// Must match IDeviceRouting in Device.tsx
    /// </summary>
    public class JsonDeviceRouting(string address, IReadOnlyList<JsonDeviceRoute> routing)
    {
        public string Address { get; } = address;
        public IReadOnlyList<JsonDeviceRoute> Routing = routing;
    }

    public class JsonDeviceRoute(string sourceEvent, string targetAddress, string targetFunctionality) : IEventRoute
    {
        public string SourceEvent { get; } = sourceEvent;
        public string TargetAddress { get; } = targetAddress;
        public string TargetFunctionality { get; } = targetFunctionality;
    }

    public class JsonDeviceRoutingOptions(string address, IReadOnlyList<JsonDeviceProvidedEvent> providedEvents, IReadOnlyList<JsonDeviceConsumableEvent> consumedEvents)
    {
        public string Address { get; } = address;
        public IReadOnlyList<JsonDeviceProvidedEvent> ProvidedEvents { get; } = providedEvents;
        public IReadOnlyList<JsonDeviceConsumableEvent> ConsumableEvents { get; } = consumedEvents;
    }

    internal class JsonDeviceConsumableEvent(string eventType, string targetName)
    {
        public string EventType { get; } = eventType;
        public string TargetName { get; } = targetName;
    }

    internal class JsonDeviceProvidedEvent(string eventType, string name)
    {
        public string EventType { get; } = eventType;
        public string Name { get; } = name;
    }
}
