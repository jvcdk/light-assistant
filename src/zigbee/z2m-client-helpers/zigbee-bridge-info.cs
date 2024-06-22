using LightAssistant.Interfaces;
using Newtonsoft.Json;

namespace LightAssistant.Zigbee;

internal partial class Zigbee2MqttClient
{
    public class Device : IDevice
    {
        private string _name = "";
        [JsonProperty("friendly_name")]
        public string Name {
            get => _name;
            set => _name = value ?? "";
        }

        private string _address = "";
        [JsonProperty("ieee_address")]
        public string Address {
            get => _address;
            set => _address = value ?? "";
        }

        private string _vendor = "";
        [JsonProperty("manufacturer")]
        public string Vendor {
            get => _vendor;
            set => _vendor = value ?? "";
        }

        private string _model = "";
        [JsonProperty("model_id")]
        public string Model {
            get => _model;
            set => _model = value ?? "";
        }

        [JsonProperty("definition")]
        public DeviceDefinition? Definition { get; set; }

        [JsonIgnore]
        public string Description => Definition?.Description ?? "";

        private string _powerSource = "";
        [JsonProperty("power_source")]
        public string PowerSource {
            get => _powerSource;
            set => _powerSource = value ?? "";
        }

        [JsonIgnore]
        public bool BatteryPowered => PowerSource == "Battery";

        bool IDevice.Equals(IDevice other)
        {
            return
                Name == other.Name &&
                Address == other.Address &&
                Vendor == other.Vendor &&
                Model == other.Model &&
                Description == other.Description &&
                BatteryPowered == other.BatteryPowered;
        }

        internal Func<string, Dictionary<string, string>, Task>? SendToBus { get; set; } 

        public void SendBrightnessTransition(int brightness, int transitionTime)
        {
            var data = new Dictionary<string, string> {
                ["transition"] = transitionTime.ToString(), 
            };

            if(brightness == 0)
                data["state"] = "OFF";
            else {
                data["brightness"] = brightness.ToString();
                data["state"] = "ON";
            }
            SendToBus?.Invoke($"{Address}/set", data);
        }
    }

    public class DeviceDefinition
    {
        private string _description = "";

        [JsonProperty("description")]
        public string Description { 
            get => _description;
            set => _description = value ?? "";
         }
    }

    public class GenericMqttResponse
    {
        private Dictionary<string, string> _data = [];

        [JsonProperty("data")]
        public Dictionary<string, string> Data {
            get => _data;
            set => _data = value ?? [];
        }

        private string _status = "";

        [JsonProperty("status")]
        public string Status {
            get => _status;
            set => _status = value ?? "";
        }
    }
}
