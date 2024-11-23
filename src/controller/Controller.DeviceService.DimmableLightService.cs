using System.Diagnostics;
using System.Timers;
using LightAssistant.Interfaces;
using Timer = System.Timers.Timer;

namespace LightAssistant.Controller;

internal partial class Controller
{
    private abstract partial class DeviceService
    {
        internal class DimmableLightService : DeviceService
        {
            private readonly object _lock = new();

            private const double FastTransitionTime = 0.25; // s
            private const double SlowTransitionTime = 1.25; // s
            private const int LastStepTimeConstantMs = 350;
            private const int StepSizeScaling = 100;
            private const int FadeInitiateDelayMs = 750;
            private const int FadeTimeFactor = 10000; // Scaling factor Normalized StepSize => Fade time in seconds.
            private const double FadeMinIntervalMs = 500; // Max 2 messages pr second

            private readonly Timer _fadeTriggerTimer = new();
            private readonly Timer _fadeEngineTimer = new();
            private double _fadeBrightnessTarget = 0;
            private double _upcomingFadeTimeValue = 0; // Doesn't really have a unit. It is Button StepSize but is scaled into seconds.
            private double _fadeTime; // Unit: s
            private double _fadeNextBrightness;

            private readonly int MaxBrightness;
            private int Brightness => (int)Math.Round(ApplyGamma(_brightness) * MaxBrightness);

            private double _brightness = 0.0; // Don't set this directly; use SetPrivateBrightness
            private double _lastSteadyStateBrightness = 1.0;

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

            private bool IsOn => _brightness > float.Epsilon;
            private bool IsFullOn => (1.0 - _brightness) < float.Epsilon;
            private double UnApplyGamma(double value) => Math.Pow(value, 1.0 / _gamma);
            private double ApplyGamma(double value) => Math.Pow(value, _gamma);

            public DimmableLightService(IDevice device, int maxBrightness) : base("", device)
            {
                MaxBrightness = maxBrightness;

                _fadeTriggerTimer.Elapsed += TriggerFade;
                _fadeTriggerTimer.AutoReset = false;
                _fadeTriggerTimer.Interval = FadeInitiateDelayMs;

                _fadeEngineTimer.Elapsed += (sender, args) => RunFade();
                _fadeEngineTimer.AutoReset = false;
            }

            internal override IEnumerable<InternalEventSink> ConsumedEvents => [
                new InternalEventSink(typeof(InternalEvent_Push), "Toggle on/off", HandleToggleOnOff),
                new InternalEventSink(typeof(InternalEvent_Rotate), "Dim", HandleDim),
                new InternalEventSink(typeof(InternalEvent_Rotate), "Fade", HandleFade)
            ];

            private void SetPrivateBrightness(double value, bool directionIsUp, double transitionTime = FastTransitionTime)
            {
                if(directionIsUp) {
                    _brightness = Math.Min(value, 1.0);
                    if(Brightness == 0)
                        _brightness = UnApplyGamma(1.0 / MaxBrightness);
                }
                else {
                    _brightness = Math.Max(value, 0.0);
                    if(Brightness == 0)
                        _brightness = 0;
                }
                Device.SendBrightnessTransition(Brightness, transitionTime);
            }

            private void HandleToggleOnOff(InternalEvent ev)
            {
                Debug.Assert(ev.GetType() == typeof(InternalEvent_Push));

                HandleTurnOnOff(new ActionEvent_TurnOnOff {
                    UserGenerated = true,
                    Mode = TurnOnOffModes.Toggle
                });
            }

            [ActionSink("Turn on/off")]
            private void HandleTurnOnOff(ActionEvent_TurnOnOff ev)
            {
                lock (_lock) {
                    if (!IsDoneFading) {
                        _fadeBrightnessTarget = _brightness;
                        if(ev.UserGenerated)
                            return;
                    }

                    if(ev.Mode == TurnOnOffModes.TurnOn && IsOn)
                        return;

                    if(ev.Mode == TurnOnOffModes.TurnOff && !IsOn)
                        return;

                    var oldBrightness = _brightness;
                    if (IsOn) {
                        _lastSteadyStateBrightness = _brightness;
                        _brightness = 0;
                    }
                    else
                        _brightness = _lastSteadyStateBrightness;

                    Device.SendBrightnessTransition(Brightness, CalcTransitionTime(oldBrightness, SlowTransitionTime));
                }
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

                lock(_lock) {
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
                    _fadeBrightnessTarget = _brightness;
                }
            }

            private void HandleFade(InternalEvent ev)
            {
                if(ev is not InternalEvent_Rotate evRotate) {
                    Debug.Assert(false);
                    return;
                }

                lock(_lock) {
                    if(evRotate.IsUp)
                        _upcomingFadeTimeValue += evRotate.StepSize;
                    else
                        _upcomingFadeTimeValue -= evRotate.StepSize;

                    _fadeTriggerTimer.Stop();
                    _fadeTriggerTimer.Start();
                }
            }

            private void TriggerFade(object? sender, ElapsedEventArgs args)
            {
                ActionEvent_FadeToBrightness ev;
                lock(_lock) {
                    var directionIsUp = _upcomingFadeTimeValue > 0;
                    ev = new ActionEvent_FadeToBrightness {
                        Brightness = directionIsUp ? 1.0 : 0.0,
                        Duration = (int) Math.Round(FadeTimeFactor * Math.Abs(_upcomingFadeTimeValue) / StepSizeScaling), // Unit: s,
                        UserGenerated = true
                    };
                    _upcomingFadeTimeValue = 0;
                }
                HandleFade(ev);
            }

            [ActionSink("Fade to brightness")]
            private void HandleFade(ActionEvent_FadeToBrightness ev)
            {
                lock(_lock) {
                    if(ev.Duration <= 0)
                        return;

                    _fadeBrightnessTarget = ev.Brightness;
                    var distance = ev.UserGenerated ? 1:
                        Math.Abs(_brightness - ev.Brightness);
                    _fadeTime = ev.Duration * distance; // Unit: s
                    RunFade();
                }
            }

            private bool IsDoneFading => Math.Abs(_fadeBrightnessTarget - _brightness) < float.Epsilon;

            private void RunFade()
            {
                lock(_lock) {
                    if(IsDoneFading)
                        return;

                    var isUp = _fadeBrightnessTarget > _brightness;
                    var target = (int) Math.Round(MaxBrightness * _fadeBrightnessTarget);
                    double intervalMs = CalculateInterval(isUp, target);
                    SetPrivateBrightness(_fadeNextBrightness, isUp, intervalMs / 1000);

                    if (IsDoneFading)
                        return;

                    _fadeEngineTimer.Interval = intervalMs;
                    _fadeEngineTimer.Start();
                }

                double CalcNextBrightnessStepValue(int step) => UnApplyGamma((double)(Brightness + step) / MaxBrightness);

                double CalculateInterval(bool isUp, int target)
                {
                    double intervalMs = 0;
                    var direction = isUp ? 1 : -1;
                    var step = direction;
                    do {
                        _fadeNextBrightness = CalcNextBrightnessStepValue(step);
                        var delta = Math.Abs(_fadeNextBrightness - _brightness);
                        intervalMs = delta * _fadeTime * 1000;
                        step += direction;
                    } while (intervalMs < FadeMinIntervalMs && (Brightness + step) != target);

                    if (Brightness == 0 && isUp)
                        intervalMs = 1; // Instant on, timer requires at least 1 ms.
                    return intervalMs;
                }
            }
        }
    }
}
