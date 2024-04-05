
namespace LightAssistant.Interfaces
{
    internal interface IEventRoute
    {
        internal string SourceAddress { get; }
        internal string SourceEvent { get; }
        internal string TargetAddress { get; }
        internal string TargetFunctionality { get; }
    }

    internal interface IRoutingOptions
    {
        internal IReadOnlyList<string> ProvidedEvents { get; }
        internal IReadOnlyList<IConsumableEvent> ConsumedEvents { get; }
    }

    internal interface IConsumableEvent
    {
        internal string Type { get; }
        internal string Functionality { get; }
    }
}
