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
}
