using Newtonsoft.Json;

namespace LightAssistant.WebApi;

internal partial class WebApi
{
    private class CustomConverter<TInterface, TImplementation> : JsonConverter where TImplementation : TInterface, new()
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(TInterface);

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) =>
            serializer.Deserialize<TImplementation>(reader);

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) =>
            serializer.Serialize(writer, value);
    }
}
