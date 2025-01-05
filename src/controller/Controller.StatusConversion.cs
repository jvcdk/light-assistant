namespace LightAssistant.Controller;

internal partial class Controller
{
    private class StatusConversion(string name, Func<string, string> convert)
    {
        internal string Name { get; } = name;
        internal Func<string, string> Convert { get; } = convert;

        private static readonly Dictionary<string, StatusConversion> _data = new() {
            ["brightness"] = new StatusConversion("Brightness", ConvertIdentity),
            ["battery"] = new StatusConversion("Battery", ConvertPercent),
            ["voltage"] = new StatusConversion("State", ConvertMvToV),
            ["linkquality"] = new StatusConversion("Link quality", ConvertIdentity),
            ["state"] = new StatusConversion("State", ConvertBool),
        };

        internal static Dictionary<string, string> ExctractStatus(Dictionary<string, string> data)
        {
            var result = new Dictionary<string, string>();
            foreach(var kv in data) {
                if(!_data.TryGetValue(kv.Key, out var conversion))
                    continue;

                result[conversion.Name] = conversion.Convert(kv.Value);
            }
            return result;
        }

        private static string ConvertIdentity(string value) => value;

        private static string ConvertBool(string value) {
            value = value.ToLower();
            var isTrue = value == "1" || value == "true" || value == "on" || value == "yes";
            return isTrue ? "On" : "Off";
        }

        private static string ConvertMvToV(string value) {
            if(!double.TryParse(value, out var mv))
                return value;
            return (mv / 1000).ToString("F2") + "V";
        }

        private static string ConvertPercent(string value) => value + "%";
    }
}
