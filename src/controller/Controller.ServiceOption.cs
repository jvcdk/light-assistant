using LightAssistant.Interfaces;

namespace LightAssistant.Controller;

internal partial class Controller
{
    private class ServiceOption(string name, object value, ParamDescriptor @param, Action<string> action) : IServiceOption
    {
        public ParamInfo Param { get; } = new(name, @param);
        public object Value { get; } = value;
        public Action<string> Action { get; } = action;
    }
}
