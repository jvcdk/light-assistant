namespace LightAssistant.Interfaces;

internal interface IServicePreviewOption
{
    void PreviewDeviceOption(string value, PreviewMode previewMode);
}

internal interface IServiceOption
{
    ParamInfo Param { get; }
    object Value { get; }
}

internal interface IServiceOptionValue
{
    string Name { get; }
    string Value { get; }
}
