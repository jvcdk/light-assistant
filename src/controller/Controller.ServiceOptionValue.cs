using LightAssistant.Interfaces;

namespace LightAssistant.Controller;

internal partial class Controller
{
    internal class ServiceOptionValue(string name, string value) : IServiceOptionValue
    {
        public string Name { get; set; } = name;
        public string Value { get; set; } = value;

        public ServiceOptionValue(ServiceOptionValue source) : this(source.Name, source.Value) { }

        public ServiceOptionValue() : this ("", "") { }
    }

    private class ServiceOptionValues : Dictionary<string, IReadOnlyList<ServiceOptionValue>>
    {
    }
}
