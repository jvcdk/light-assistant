using LightAssistant.Utils;
using Newtonsoft.Json;

namespace LightAssistant;

internal class ParamInfo(string name, ParamDescriptor param)
{
    public string Name { get; } = name;
    public ParamDescriptor Param { get; } = param;
}


[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
[JsonObject(MemberSerialization.OptIn)] // Use opt-in strategy to avoid properties from Attribute base class in serialization
internal abstract class ParamDescriptor : Attribute
{
    [JsonProperty]
    public abstract string Type {get;}
}

internal class ParamEnum : ParamDescriptor
{
    [JsonProperty]
    public string[] Values { get; }

    [JsonProperty]
    public string Default => (Values.Length > 0) ? Values[0] : string.Empty;

    internal ParamEnum(Type sourceEnum)
    {
        if (!sourceEnum.IsEnum)
            throw new ArgumentException("SourceEnum must be an enum type.");

        Values = Enum.GetNames(sourceEnum).Select(entry => entry.CamelCaseToSentence()).ToArray();
    }

    public override string Type => "enum";
}

internal class ParamFloat(double min, double max) : ParamDescriptor
{
    [JsonProperty]
    public double Min { get; } = min;

    [JsonProperty]
    public double Max { get; } = max;

    [JsonProperty]
    public double Default => (Min + Max) / 2.0;

    public override string Type => "float";
}

internal class ParamBrightness() : ParamFloat(0.0, 1.0)
{
    public override string Type => "brightness";
}
