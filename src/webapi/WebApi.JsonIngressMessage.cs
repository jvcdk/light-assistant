using Newtonsoft.Json;
using static System.Text.Encoding;

namespace LightAssistant.WebApi;

internal partial class WebApi
{
    // TODO JVC: Align with tsx code
    public class JsonIngressMessage
    {
        public JsonDeviceConfigurationChange? DeviceConfigurationChange { get; set; }
    }

    public class JsonDeviceConfigurationChange
    {
        public string Address { get; init; } = "";
        public string Name { get; init; } = "";
        public JsonDeviceRoute[] Route { get; init; } = [];
    }

    public static class JsonIngressMessageParser
    {
        public static JsonIngressMessage? ParseMessage(string msg)
        {
            return JsonConvert.DeserializeObject<JsonIngressMessage>(msg);
        }
    }
}
