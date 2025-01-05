namespace LightAssistant.Controller;

internal class DeviceStatusConverter
{
    private readonly Dictionary<string, Converter> _data = [];

    internal Dictionary<string, string> Process(Dictionary<string, string> data)
    {
        var result = new Dictionary<string, string>();
        foreach (var kv in data.OrderBy(kv => kv.Key)) {
            if (!_data.TryGetValue(kv.Key, out var conversion))
                continue;

            result[conversion.Name] = conversion.Convert(kv.Value);
        }
        return result;
    }

    internal enum Types { Identity, Bool, MvToV, Percent }

    internal DeviceStatusConverter With(string key, string name, Types type)
    {
        Func<string, string> convert = type switch {
            Types.Identity => Converter.ConvertIdentity,
            Types.Bool => Converter.ConvertBool,
            Types.MvToV => Converter.ConvertMvToV,
            Types.Percent => Converter.ConvertPercent,
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
        _data[key] = new Converter(name, convert);
        return this;
    }


    private class Converter(string name, Func<string, string> convert)
    {
        internal string Name { get; } = name;
        internal Func<string, string> Convert { get; } = convert;

        internal static string ConvertIdentity(string value) => value;

        internal static string ConvertBool(string value)
        {
            value = value.ToLower();
            var isTrue = value == "1" || value == "true" || value == "on" || value == "yes";
            return isTrue ? "On" : "Off";
        }

        internal static string ConvertMvToV(string value)
        {
            if (!double.TryParse(value, out var mv))
                return value;
            return (mv / 1000).ToString("F2") + "V";
        }

        internal static string ConvertPercent(string value) => value + "%";
    }
}
