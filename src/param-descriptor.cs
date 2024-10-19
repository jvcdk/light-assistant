using LightAssistant.Utils;

namespace LightAssistant;

internal class ParamInfo(string name, ParamDescriptor param)
{
    public string Name { get; } = name;
    public ParamDescriptor Param { get; } = param;
}


[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
internal abstract class ParamDescriptor : Attribute
{
}

internal class ParamEnum : ParamDescriptor
{
    public string[] Values { get; }

    public string Default => (Values.Length > 0) ? Values[0] : string.Empty;

    internal ParamEnum(Type sourceEnum)
    {
        if (!sourceEnum.IsEnum)
            throw new ArgumentException("SourceEnum must be an enum type.");

        Values = Enum.GetNames(sourceEnum).Select(entry => entry.CamelCaseToSentence()).ToArray();
    }
}

internal class ParamFloat(double min, double max) : ParamDescriptor
{
    public double Min { get; } = min;

    public double Max { get; } = max;

    public double Default => (Min + Max) / 2.0;
}

internal class ParamBrightness() : ParamFloat(0.0, 1.0)
{
}
