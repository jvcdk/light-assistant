using System.Diagnostics;
using LightAssistant.Interfaces;

namespace LightAssistant.Controller;

internal partial class Controller
{
    private abstract partial class DeviceService : IDisposable
    {
        internal class DimmableLightService(IDevice device, int maxBrightness) : DeviceService("", device)
        {
            private const double FastTransitionTime = 0.25; // s
            private const double SlowTransitionTime = 1.25; // s
            private const int LastStepTimeConstantMs = 350;
            private const int StepSizeScaling = 100;

            private readonly int MaxBrightness = maxBrightness;
            private int Brightness => (int)Math.Round(Math.Pow(_brightness, _gamma) * MaxBrightness);

            private double _brightness = 0.0; // Don't set this directly; use SetPrivateBrightness
            private double _lastSteadyStateBrightess = 1.0;

            private double _gamma = 10; // Range: _gamma > double.Epsilon. TODO: Make a user interface for configuring gamma.
            internal double Gamma {    // Range: Gamma <= 1.0 || Gamma >= 1.0
                get {
                    if(_gamma >= 1.0)
                        return _gamma;
                    return -1.0 / _gamma;
                }
                set {
                    var absValue = Math.Max(Math.Abs(value), 1.0);
                    if(value >= 0)
                        _gamma = absValue;
                    else
                        _gamma = 1.0 / absValue;
                }
            }


            internal override IEnumerable<InternalEventSink> ConsumedEvents => [
                new InternalEventSink(typeof(InternalEvent_Push), "ToggleOnOff", HandleToggleOnOff),
                new InternalEventSink(typeof(InternalEvent_Rotate), "Dim", HandleDim),
                new InternalEventSink(typeof(InternalEvent_Rotate), "Fade", HandleFade)
            ];

            private void SetPrivateBrightness(double value, bool directionIsUp)
            {
                if(directionIsUp) {
                    _brightness = Math.Min(value, 1.0);
                    if(Brightness == 0)
                        _brightness = Math.Pow(1.0 / MaxBrightness, 1.0 / _gamma);
                }
                else {
                    _brightness = Math.Max(value, 0.0);
                    if(Brightness == 0)
                        _brightness = 0;
                }

            }

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

                if(evRotate.IsUp)
                    _brightnessStep += evRotate.StepSize;
                else
                    _brightnessStep -= evRotate.StepSize;

                SetPrivateBrightness(_brightness + _brightnessStep / StepSizeScaling, evRotate.IsUp);

                Device.SendBrightnessTransition(Brightness, FastTransitionTime);
            }

            private void HandleFade(InternalEvent ev)
            {
            }

            private bool IsOn => _brightness > 0;
        }
    }
}
