using System.Runtime.InteropServices.JavaScript;
using LightAssistant.Interfaces;
using Newtonsoft.Json;
using static System.Text.Encoding;

namespace LightAssistant.WebApi;

internal partial class WebApi
{
    public abstract class JsonMessage
    {
        internal byte[] Serialize() => UTF8.GetBytes(JsonConvert.SerializeObject(this));
    }

    public class JsonMessageDeviceList : JsonMessage
    {
        public IReadOnlyList<JsonDevice> Devices { get; }

        public JsonMessageDeviceList(IReadOnlyList<IDevice> devices)
        {
            Devices = devices.Select(JsonDevice.FromIDevice).ToList();
        }
    }

    /// <summary>
    /// Serializing an interface (i.e. IDevice) directly, does not produce json information for the interface,
    /// but for the implementing object. Hence we need to create a specific object for IDevice.
    /// </summary>
    public class JsonDevice : IDevice
    {
        public string Name { get; }
        public string Address { get; }
        public string Vendor { get; }
        public string Model { get; }
        public string Description { get; }

        public JsonDevice(IDevice source)
        {
            Name = source.Name;
            Address = source.Address;
            Vendor = source.Vendor;
            Model = source.Model;
            Description = source.Description;
        }

        public static JsonDevice FromIDevice(IDevice source) => new JsonDevice(source);
    }
}

