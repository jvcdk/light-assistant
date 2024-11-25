using LightAssistant.Interfaces;
using LightAssistant.Utils;

namespace LightAssistant.Controller;

internal partial class Controller
{
    private abstract partial class DeviceService : IDisposable
    {
        private const string KeywordAction = "action";
        private const string KeywordActionStepSize = "action_step_size";

        protected IConsoleOutput ConsoleOutput { get; }
        internal string Name { get; private set; }

        protected readonly IDevice Device;
        private readonly Thread? _processingThread;
        private volatile bool _terminate = false;
        private readonly Mutex _flag = new();

        protected DeviceService(string name, IDevice device, IConsoleOutput consoleOutput)
        {
            Name = name;
            Device = device;
            ConsoleOutput = consoleOutput;
            if(NeedsTickCall) {
                _processingThread = new(ProcessingThread);
                _processingThread.Start();
            }
        }

        private void ProcessingThread(object? obj)
        {
            while(!_terminate) {
                int delayMs = ProcessTick();
                _flag.WaitOne(delayMs);
            }
        }

        protected virtual bool NeedsTickCall => false;

        protected void WakeUpProcessing() => _flag.ReleaseMutex();

        protected virtual int ProcessTick() => -1;

        public void Dispose()
        {
            _terminate = true;
            WakeUpProcessing();
            _processingThread?.Join();
        }

        internal virtual IEnumerable<InternalEvent> ProcessExternalEvent(IDevice sourceDevice, IReadOnlyDictionary<string, string> data)
        {
            foreach(var service in EnumerateServices()) {
                foreach(var ev in service.ProcessExternalEvent(sourceDevice, data))
                    yield return ev;
            }
        }

        internal virtual void ProcessInternalEvent(InternalEvent ev, string targetFunctionality)
        {
            foreach(var evDst in ConsumedEvents)
                if(evDst.TargetName == targetFunctionality)
                    evDst.Handler(ev);
        }

        private IEnumerable<DeviceService> EnumerateServices() => this.EnumeratePropertiesOfType<DeviceService>();

        internal virtual IEnumerable<InternalEventSink> ConsumedEvents {
            get => EnumerateServices().SelectMany(service => service.ConsumedEvents);
        }

        internal virtual IEnumerable<InternalEventSource> ProvidedEvents =>
            EnumerateServices().SelectMany(service => service.ProvidedEvents);
    }
}
