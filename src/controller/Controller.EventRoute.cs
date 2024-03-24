using LightAssistant.Interfaces;

namespace LightAssistant.Controller;

internal partial class Controller
{
    private class EventRoute(string sourceAddress, string sourceEvent, string targetAddress, string targetFunctionality) : IEventRoute
    {
        public string SourceAddress { get; } = sourceAddress;
        public string SourceEvent { get; } = sourceEvent;
        public string TargetAddress { get; } = targetAddress;
        public string TargetFunctionality { get; } = targetFunctionality;
    }
}
