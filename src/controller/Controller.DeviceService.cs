using System.Diagnostics;
using LightAssistant.Interfaces;
using LightAssistant.Utils;

namespace LightAssistant.Controller;

internal partial class Controller
{
    private abstract class DeviceService : IDisposable
    {
        private const string KeywordAction = "action";
        private const string KeywordActionStepSize = "action_step_size";

        internal string Name { get; private set; }

        protected readonly IDevice Device;
        private readonly Thread? _processingThread;
        private volatile bool _terminate = false;
        private readonly Mutex _flag = new();

        protected DeviceService(string name, IDevice device)
        {
            Name = name;
            Device = device;
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


        internal class DimmableLightService(IDevice device, int maxBrightness) : DeviceService("", device)
        {
            private const double FastTransitionTime = 0.25; // s
            private const double SlowTransitionTime = 1.25; // s
            private const int LastStepTimeConstantMs = 350;
            private const int StepSizeScaling = 100;

            private readonly int MaxBrightness = maxBrightness;
            private int Brightness => (int) Math.Round(_brightness * MaxBrightness);

            private double _brightness = 0.0;
            private double _lastSteadyStateBrightess = 1.0;


            internal override IEnumerable<InternalEventSink> ConsumedEvents => [
                new InternalEventSink(typeof(InternalEvent_Push), "ToggleOnOff", HandleToggleOnOff),
                new InternalEventSink(typeof(InternalEvent_Rotate), "Dim", HandleDim),
                new InternalEventSink(typeof(InternalEvent_Rotate), "Fade", HandleFade)
            ];

            private void HandleToggleOnOff(InternalEvent ev)
            {
                Debug.Assert(ev.GetType() == typeof(InternalEvent_Push));

                var oldBrightness = _brightness;
                if (IsOn) {
                    _lastSteadyStateBrightess = _brightness;
                    _brightness = 0;
                }
                else {
                    _brightness = _lastSteadyStateBrightess;
                }
                Device.SendBrightnessTransition(Brightness, CalcTransitionTime(oldBrightness, SlowTransitionTime));
            }

            private double CalcTransitionTime(double oldBrightness, double maxTransitionTime) => Math.Abs(oldBrightness - _brightness) * maxTransitionTime;

            private long _lastRotateEvent = 0;
            private double _brightnessStep = 0;
            private void HandleDim(InternalEvent ev)
            {
                if(ev is not InternalEvent_Rotate evRotate) {
                    Debug.Assert(false);
                    return;
                }

                var lastDirectionUp = _brightnessStep > 0;
                var isSameDirection = evRotate.IsUp == lastDirectionUp;
                var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                if(isSameDirection) {
                    var deltaMs = now - _lastRotateEvent;
                    _brightnessStep *= Math.Exp(-deltaMs / LastStepTimeConstantMs);
                }
                else
                    _brightnessStep = 0;
                _lastRotateEvent = now;

                if(evRotate.IsUp) {
                    _brightnessStep += evRotate.StepSize;
                    _brightness = Math.Min(_brightness + _brightnessStep / StepSizeScaling, 1.0);
                }
                else {
                    _brightnessStep -= evRotate.StepSize;
                    _brightness = Math.Max(_brightness + _brightnessStep / StepSizeScaling, 0.0);
                }

                Device.SendBrightnessTransition(Brightness, FastTransitionTime);
            }

            private void HandleFade(InternalEvent ev)
            {
            }

            private bool IsOn => _brightness > 0;
        }

        internal class PushService(string path, IDevice device) : DeviceService(path, device)
        {
            public string Push { get; set; } = string.Empty;

            internal override IEnumerable<InternalEventSource> ProvidedEvents => [
                new InternalEventSource(typeof(InternalEvent_Push), Name)
            ];

            internal override IEnumerable<InternalEvent> ProcessExternalEvent(IDevice sourceDevice, IReadOnlyDictionary<string, string> data)
            {
                if(string.IsNullOrWhiteSpace(Push))
                    yield break;

                if(data.TryGetValue(KeywordAction, out var value) && value == Push)
                    yield return new InternalEvent_Push(sourceDevice.Address, Name);
            }
        }

        internal class RotateService(string name, IDevice device) : DeviceService(name, device)
        {
            public string RotateRight { get; init; } = string.Empty;
            public string RotateLeft { get; init; } = string.Empty;
            public double UnitStepSize { get; init; }

            internal override IEnumerable<InternalEventSource> ProvidedEvents => [
                new InternalEventSource(typeof(InternalEvent_Rotate), Name)
            ];

            internal override IEnumerable<InternalEvent> ProcessExternalEvent(IDevice sourceDevice, IReadOnlyDictionary<string, string> data)
            {
                if(!data.TryGetValue(KeywordActionStepSize, out var stepSizeStr))
                    yield break;;

                if(!int.TryParse(stepSizeStr, out var stepSizeInt))
                    yield break;;

                var stepSize = stepSizeInt / UnitStepSize;
                if(Match(data, RotateRight))
                    yield return new InternalEvent_Rotate(sourceDevice.Address, Name) {
                        StepSize = stepSize,
                        IsUp = true,
                    };
                else if(Match(data, RotateLeft))
                    yield return new InternalEvent_Rotate(sourceDevice.Address, Name) {
                        StepSize = stepSize,
                        IsUp = false,
                    };
            }

            private static bool Match(IReadOnlyDictionary<string, string> haystack, string needle)
            {
                return !string.IsNullOrWhiteSpace(needle) &&
                    haystack.TryGetValue(KeywordAction, out var value) &&
                    needle == value;
            }
        }

        internal class SmartKnobService: DeviceService
        {
            internal SmartKnobService(IDevice device, string path, string actionPush, 
                string actionNormalRotateLeft, string actionNormalRotateRight,
                string actionPushedRotateLeft, string actionPushedRotateRight,
                double unitStepSize) : base(path, device)
            {
                Button = new PushService("Push", device) { Push = actionPush};
                Normal = new RotateService("Rotate normal", device) { RotateLeft = actionNormalRotateLeft, RotateRight = actionNormalRotateRight, UnitStepSize = unitStepSize };
                Pushed = new RotateService("Rotate pushed", device) { RotateLeft = actionPushedRotateLeft, RotateRight = actionPushedRotateRight, UnitStepSize = unitStepSize };
            }

            public PushService Button { get; private set; }
            public RotateService Normal { get; private set; }
            public RotateService Pushed { get; private set; }
        }

        internal class AutoModeChangeService(IDevice device) : DeviceService("", device)
        {
            public string ModeField { get; set; } = string.Empty;
            public string FromMode { get; set; } = string.Empty;
            public string ModeChangeCommand { get; set; } = string.Empty;
        }
    }
}
