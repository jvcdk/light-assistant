using LightAssistant.Interfaces;
using Newtonsoft.Json;
using static System.Text.Encoding;

namespace LightAssistant.WebApi;

internal partial class WebApi
{
    public class JsonMessage
    {
        internal byte[] Serialize() => UTF8.GetBytes(JsonConvert.SerializeObject(this));

        public IReadOnlyList<JsonDevice>? Devices { get; private set; }
        public JsonDeviceStatus? DeviceStatus { get; private set; }
        public JsonDeviceRouting? Routing { get; private set; }
        public JsonDeviceRoutingOptions? RoutingOptions { get; private set; }

        internal static JsonMessage CreateDeviceList(IReadOnlyList<IDevice> devices) =>
            new() {
                Devices = devices.Select(JsonDevice.FromIDevice).ToList()
            };

        internal void AddDeviceStatus(string address, IDeviceStatus status) => 
            DeviceStatus = new JsonDeviceStatus(address, status);

        internal void AddDeviceRouting(string address, IReadOnlyList<JsonDeviceRoute> routing) => 
            Routing = new JsonDeviceRouting(address, routing);

        internal void AddDeviceRoutingOptions(string address, IReadOnlyList<string> providedEvents, IReadOnlyList<JsonDeviceConsumableEvent> consumableEvents) => 
            RoutingOptions = new JsonDeviceRoutingOptions(address, providedEvents, consumableEvents);

        internal static JsonMessage CreateDeviceStatus(string address, IDeviceStatus deviceStatus) => 
            new() { DeviceStatus = new JsonDeviceStatus(address, deviceStatus)};
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

    public class JsonDeviceRoute(string sourceEvent, string targetAddress, string targetFunctionality)
    {
        public string SourceEvent { get; } = sourceEvent;
        public string TargetAddress { get; } = targetAddress;
        public string TargetFunctionality { get; } = targetFunctionality;
    }

    public class JsonDeviceRoutingOptions(string address, IReadOnlyList<string> providedEvents, IReadOnlyList<JsonDeviceConsumableEvent> consumedEvents)
    {
        public string Address { get; } = address;
        public IReadOnlyList<string> ProvidedEvents { get; } = providedEvents;
        public IReadOnlyList<JsonDeviceConsumableEvent> ConsumableEvents { get; } = consumedEvents;
    }

    internal class JsonDeviceConsumableEvent(string eventName, string targetName)
    {
        public string EventName { get; } = eventName;
        public string TargetName { get; } = targetName;
    }
}
