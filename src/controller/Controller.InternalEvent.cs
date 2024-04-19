namespace LightAssistant.Controller;

internal partial class Controller
{
    private abstract class InternalEvent(string sourceAddress, string eventPath)
    {
        public string SourceAddress { get; } = sourceAddress;
        public string EventPath { get; } = eventPath;

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

    private class InternalEvent_Push(string sourceAddress, string eventPath) : InternalEvent(sourceAddress, eventPath)
    {
    }

    private class InternalEvent_Rotate(string sourceAddress, string eventPath) : InternalEvent(sourceAddress, eventPath)
    {
    }

    private class InternalEventSink
    {
        internal readonly Type EventType;
        internal readonly string TargetName;

        internal InternalEventSink(Type ev, string target)
        {
            if(!ev.IsSubclassOf(typeof(InternalEvent)))
                throw new Exception("Invalid Sink type. It should inherit from InternalEvent.");

            EventType = ev;
            TargetName = target;
        }
    }

    private class InternalEventSource
    {
        internal readonly Type EventType;
        internal readonly string Path;

        internal InternalEventSource(Type ev, string path)
        {
            if(!ev.IsSubclassOf(typeof(InternalEvent)))
                throw new Exception("Invalid Source type. It should inherit from InternalEvent.");

            EventType = ev;
            Path = path;
        }
    }
}
