using LightAssistant.Interfaces;

namespace LightAssistant.Controller;

internal partial class Controller
{
    internal class EventRoute(string sourceEvent, string targetAddress, string targetFunctionality) : IEventRoute
    {
        public EventRoute(IEventRoute source) : this(source.SourceEvent, source.TargetAddress, source.TargetFunctionality) { }

        public string SourceEvent { get; } = sourceEvent;
        public string TargetAddress { get; } = targetAddress;
        public string TargetFunctionality { get; } = targetFunctionality;
    }

    private class RoutingOptions(IReadOnlyList<IProvidedEvent> providedEvents, IReadOnlyList<IConsumableEvent> consumedEvents, List<ConsumableTrigger> consumedTriggers) : IRoutingOptions
    {
        public IReadOnlyList<IProvidedEvent> ProvidedEvents { get; } = providedEvents;
        public IReadOnlyList<IConsumableEvent> ConsumedEvents { get; } = consumedEvents;
        public IReadOnlyList<IConsumableTrigger> ConsumedTriggers { get; } = consumedTriggers;
    }

    private class ConsumableEvent(string type, string functionality) : IConsumableEvent
    {
        public string Type { get; } = type;
        public string Functionality { get; } = functionality;
    }

    private class ProvidedEvent(string type, string name) : IProvidedEvent
    {
        public string Type { get; } = type;
        
        public string Name { get; } = name;
    }

    private class ConsumableTrigger(string type, IReadOnlyList<ParamInfo> parameters) : IConsumableTrigger
    {
        public string Type { get; } = type;
        public IReadOnlyList<ParamInfo> Parameters { get; } = parameters;
    }
}
