
namespace LightAssistant.Interfaces
{
    internal interface IEventRoute
    {
        internal string SourceAddress { get; }
        internal string SourceEvent { get; }
        internal string TargetAddress { get; }
        internal string TargetFunctionality { get; }
    }
}
