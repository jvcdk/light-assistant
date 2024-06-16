using Newtonsoft.Json;

namespace LightAssistant.WebApi;

internal partial class WebApi
{
    public class JsonClientToServerMessage
    {
        public JsonDeviceConfigurationChange? DeviceConfigurationChange { get; set; }
        public bool RequestOpenNetwork { get; set; }
    }

    public class JsonDeviceConfigurationChange
    {
        public string Address { get; init; } = "";
        public string Name { get; init; } = "";
        public JsonDeviceRoute[] Route { get; init; } = [];
    }

    public static class JsonIngressMessageParser
    {
        public static JsonClientToServerMessage? ParseMessage(string msg)
        {
            return JsonConvert.DeserializeObject<JsonClientToServerMessage>(msg);
        }
    }
}
