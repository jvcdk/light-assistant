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
    internal abstract bool Validate(string value);
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

    internal override bool Validate(string value) => Values.Contains(value);
}

internal class ParamFloat(double min, double max) : ParamDescriptor
{
    public double Min { get; } = min;

    public double Max { get; } = max;

    public double Default => (Min + Max) / 2.0;

    internal override bool Validate(string value)
    {
        if (!double.TryParse(value, out var floatValue))
            return false;

        return floatValue >= Min && floatValue <= Max;
    }
}

internal class ParamBrightness() : ParamFloat(0.0, 1.0)
{
}

internal class ParamInt(int min, int max) : ParamDescriptor
{
    public int Min { get; } = min;

    public int Max { get; } = max;

    public int Default => (int)Math.Round((Min + Max) / 2.0);

    internal override bool Validate(string value)
    {
        if (!int.TryParse(value, out var intValue))
            return false;

        return intValue >= Min && intValue <= Max;
    }
}
