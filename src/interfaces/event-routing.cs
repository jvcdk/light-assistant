
namespace LightAssistant.Interfaces
{
    internal interface IEventRoute
    {
        internal string SourceEvent { get; }
        internal string TargetAddress { get; }
        internal string TargetFunctionality { get; }
    }

    internal interface IRoutingOptions
    {
        internal IReadOnlyList<IProvidedEvent> ProvidedEvents { get; }
        internal IReadOnlyList<IConsumableEvent> ConsumedEvents { get; }
        public IReadOnlyList<IConsumableTrigger> ConsumedTriggers { get; }
    }

    internal interface IConsumableEvent
    {
        internal string Type { get; }
        internal string Functionality { get; }
    }

    internal interface IProvidedEvent
    {
        internal string Type { get; }
        internal string Name { get; }
    }

    internal interface IConsumableTrigger
    {
        internal string Type { get; }
        internal IReadOnlyList<ParamInfo> Parameters { get; }
    }
}
