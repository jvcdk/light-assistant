namespace LightAssistant.Controller;

internal partial class Controller
{
    private abstract class InternalEvent(string sourceAddress)
    {
        public string SourceAddress { get; } = sourceAddress;
        public string Type { get => GetType().ToString(); }
    }

    private class InternalEvent_ButtonPush(string sourceAddress) : InternalEvent(sourceAddress)
    {
    }

    private class InternalEvent_Rotate(string sourceAddress) : InternalEvent(sourceAddress)
    {
    }

    private class InternalEventSink
    {
        internal readonly Type Event;
        internal readonly string TargetName;

        internal InternalEventSink(Type ev, string target)
        {
            if(!ev.IsSubclassOf(typeof(InternalEvent)))
                throw new Exception("Invalid Sink type. It should inherit from InternalEvent.");

            Event = ev;
            TargetName = target;
        }
    }
}
