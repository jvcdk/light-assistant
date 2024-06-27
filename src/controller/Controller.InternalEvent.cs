namespace LightAssistant.Controller;

internal partial class Controller
{
    private abstract class InternalEvent(string sourceAddress, string serviceName)
    {
        public string SourceAddress { get; } = sourceAddress;
        public string ServiceName { get; } = serviceName;

        public string Type {
            get {
                var typeName = GetType().ToString();
                var fullName = typeof(InternalEvent).FullName ?? "";
                fullName += "_";
                if(typeName.StartsWith(fullName))
                    typeName = typeName[fullName.Length..];
                return typeName;
            }
        }
    }

    private class InternalEvent_Push(string sourceAddress, string serviceName) : InternalEvent(sourceAddress, serviceName)
    {
    }

    private class InternalEvent_Rotate(string sourceAddress, string serviceName) : InternalEvent(sourceAddress, serviceName)
    {
        internal double StepSize { get; init; }
        internal bool IsUp { get; init; } // Else: Down
    }

    private class InternalEventSink
    {
        internal readonly Type EventType;
        internal readonly string TargetName;
        internal readonly Action<InternalEvent> Handler;

        internal InternalEventSink(Type ev, string target, Action<InternalEvent> handler)
        {
            if(!ev.IsSubclassOf(typeof(InternalEvent)))
                throw new Exception("Invalid Sink type. It should inherit from InternalEvent.");

            EventType = ev;
            TargetName = target;
            Handler = handler;
        }
    }

    private class InternalEventSource
    {
        internal readonly Type EventType;
        internal readonly string Name;

        internal InternalEventSource(Type ev, string name)
        {
            if(!ev.IsSubclassOf(typeof(InternalEvent)))
                throw new Exception("Invalid Source type. It should inherit from InternalEvent.");

            EventType = ev;
            Name = name;
        }
    }
}
