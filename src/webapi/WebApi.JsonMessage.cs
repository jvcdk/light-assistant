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

        internal static JsonMessage CreateDeviceList(IReadOnlyList<IDevice> devices) =>
            new JsonMessage {
                Devices = devices.Select(JsonDevice.FromIDevice).ToList()
            };

        internal static JsonMessage CreateDeviceStatus(string address, IDeviceStatus status) =>
            new JsonMessage {
                DeviceStatus = new JsonDeviceStatus(address, status)
            };
    }

    /// <summary>
    /// Serializing an interface (i.e. IDevice) directly, does not produce json information for the interface,
    /// but for the implementing object. Hence we need to create a specific object for IDevice.
    /// 
    /// Must match IDevice in Device.tsx
    /// </summary>
    public class JsonDevice
    {
        public string Name { get; }
        public string Address { get; }
        public string Vendor { get; }
        public string Model { get; }
        public string Description { get; }
        public bool BatteryPowered { get; }

        public JsonDevice(IDevice source)
        {
            Name = source.Name;
            Address = source.Address;
            Vendor = source.Vendor;
            Model = source.Model;
            Description = source.Description;
            BatteryPowered = source.BatteryPowered;
        }

        public static JsonDevice FromIDevice(IDevice source) => new JsonDevice(source);
    }

    /// <summary>
    /// Must match IDeviceStatus in Device.tsx
    /// </summary>
    public class JsonDeviceStatus
    {
        public string Address { get; }
        public int? LinkQuality { get; }
        public int? Battery { get; }
        public int? Brightness { get; }
        public bool? State { get; }

        public JsonDeviceStatus(string address, IDeviceStatus source)
        {
            Address = address;
            LinkQuality = source.LinkQuality;
            Battery = source.Battery;
            Brightness = source.Brightness;
            State = source.State;
        }
    }
}
