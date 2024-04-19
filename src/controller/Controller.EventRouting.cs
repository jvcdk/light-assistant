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

    private class RoutingOptions(IReadOnlyList<IProvidedEvent> providedEvents, IReadOnlyList<IConsumableEvent> consumedEvents) : IRoutingOptions
    {
        public IReadOnlyList<IProvidedEvent> ProvidedEvents { get; } = providedEvents;
        public IReadOnlyList<IConsumableEvent> ConsumedEvents { get; } = consumedEvents;
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
}
