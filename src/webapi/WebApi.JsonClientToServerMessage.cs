using LightAssistant.Interfaces;
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
        public JsonDeviceScheduleEntry[] Schedule { get; init; } = [];
    }

    public static class JsonIngressMessageParser
    {
        public static JsonClientToServerMessage? ParseMessage(string msg)
        {
            var settings = GetJsonConverterSettings();
            return JsonConvert.DeserializeObject<JsonClientToServerMessage>(msg, settings);
        }

        private static JsonSerializerSettings GetJsonConverterSettings()
        {
            return new JsonSerializerSettings {
                TypeNameHandling = TypeNameHandling.Auto,
                Converters = new List<JsonConverter>
                {
                    new CustomConverter<IDeviceScheduleEntry, JsonDeviceScheduleEntry>(),
                    new CustomConverter<IScheduleTrigger, JsonScheduleTrigger>(),
                    new CustomConverter<ITimeOfDay, JsonTimeOfDay>()
                }
            };
        }
    }
}
