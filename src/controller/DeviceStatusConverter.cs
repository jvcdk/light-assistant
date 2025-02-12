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

    internal enum Types { Identity, Bool, MvToV, Percent, InvColorTemp }

    internal DeviceStatusConverter With(string key, string name, Types type)
    {
        Func<string, string> convert = type switch {
            Types.Identity => ConvertIdentity,
            Types.Bool => ConvertBool,
            Types.MvToV => ConvertMvToV,
            Types.Percent => ConvertPercent,
            Types.InvColorTemp => ConvertInvColorTemp,
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
        _data[key] = new Converter(name, convert);
        return this;
    }

    private string ConvertInvColorTemp(string arg)
    {
        if (!int.TryParse(arg, out var colorTemp))
            return arg;
        return (1000000 / colorTemp).ToString();
    }

    internal DeviceStatusConverter With(string key, string name, Func<string, string> convert)
    {
        _data[key] = new Converter(name, convert);
        return this;
    }

    private static string ConvertIdentity(string value) => value;

    private static string ConvertBool(string value)
    {
        value = value.ToLower();
        var isTrue = value == "1" || value == "true" || value == "on" || value == "yes";
        return isTrue ? "On" : "Off";
    }

    private static string ConvertMvToV(string value)
    {
        if (!double.TryParse(value, out var mv))
            return value;
        return (mv / 1000).ToString("F2") + "V";
    }

    private static string ConvertPercent(string value) => value + "%";

    private class Converter(string name, Func<string, string> convert)
    {
        internal string Name { get; } = name;
        internal Func<string, string> Convert { get; } = convert;
    }
}
