using System.Reflection;

namespace LightAssistant.Utils;

[AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field, AllowMultiple = false)]
public class NameValueAttribute : Attribute
{
    public string Name { get; }
    public string Value { get; }

    public NameValueAttribute(string nameValue)
    {
        Name = nameValue;
        Value = nameValue;
    }

    public NameValueAttribute(string name, string value)
    {
        Name = name;
        Value = value;
    }

    internal static object? ParseFromName(Type type, string name)
    {
        if(!type.IsEnum)
            return null;

        return Enum.GetValues(type)
            .Cast<Enum>()
            .Select(value => {
                var attr = GetAttribute(value);
                return new { Value = value, Name = GetName(value, attr) };
            })
            .FirstOrDefault(entry => entry.Name == name)
            ?.Value;
    }

    private static string GetName<T>(T value, NameValueAttribute? attr) =>
        attr?.Name ?? value?.ToString()?.CamelCaseToSentence() ?? "";

    internal static IEnumerable<string> GetNames(Type sourceEnum)
    {
        return Enum.GetValues(sourceEnum)
            .Cast<Enum>()
            .Select(value => {
                var attr = GetAttribute(value);
                return GetName(value, attr);
            });
    }

    private static NameValueAttribute? GetAttribute(object value) {
        var valueStr = value.ToString();
        if(valueStr == null)
            return null;
        return value.GetType().GetMember(valueStr).First().GetCustomAttribute<NameValueAttribute>();
    }

    internal static string GetValue<T>(T value) where T : notnull
    {
        var attr = GetAttribute(value);
        return GetValue(value, attr);
    }

    private static string GetValue<T>(T value, NameValueAttribute? attr) =>
        attr?.Value ?? value?.ToString()?.CamelCaseToSentence() ?? "";

    internal static string GetName<T>(T value) where T : notnull
    {
        var attr = GetAttribute(value);
        return GetName(value, attr);
    }
}
