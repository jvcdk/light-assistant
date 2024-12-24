using System.Timers;
using LightAssistant.Interfaces;
using Timer = System.Timers.Timer;

namespace LightAssistant.Controller;

internal class LightFadeEngine
{
    private const double FastTransitionTime = 0.25; // s
    private const double SlowTransitionTime = 1.25; // s
    private const int FadeInitiateDelayMs = 750;
    private const double FadeMinIntervalMs = 500; // Max 2 messages pr second
    private const double FadeSecondsPerRotation = 3600;

    private readonly Timer _fadeTriggerTimer = new();
    private readonly Timer _fadeEngineTimer = new();
    private double _fadeBrightnessTarget = 0;
    private double _upcomingFadeTime = 0; // Seconds
    private double _fadeTime; // Unit: s

    private readonly Action<double, double> _setBrightness;
    private readonly IConsoleOutput _consoleOutput;
    private readonly BrightnessConverter _brightnessConverter;
    private readonly object _lock = new();

    private double _brightness = 0.0; // Don't set this directly; use SetBrightness
    private double _lastSteadyStateBrightness = 1.0;

    internal void DimStep(double stepSize)
    {
        lock(_lock) {
            var nextStep = _brightness + stepSize;

            var direction = stepSize > 0 ? 1 : -1;
            var minNextStep = _brightness;
            _brightnessConverter.TryCalcNextStep(ref minNextStep, direction);

            if((nextStep - minNextStep) * direction < 0)
                nextStep = minNextStep;

            SetBrightness(nextStep);
            _fadeBrightnessTarget = _brightness;
        }
    }

    internal LightFadeEngine(Action<double, double> setBrightness, BrightnessConverter brightnessConverter, IConsoleOutput consoleOutput)
    {
        _setBrightness = setBrightness;
        _consoleOutput = consoleOutput;
        _brightnessConverter = brightnessConverter;

        _fadeTriggerTimer.Elapsed += TriggerFade;
        _fadeTriggerTimer.AutoReset = false;
        _fadeTriggerTimer.Interval = FadeInitiateDelayMs;

        _fadeEngineTimer.Elapsed += (sender, args) => RunFade();
        _fadeEngineTimer.AutoReset = false;
    }

    internal void TurnOnOff(Controller.TurnOnOffModes mode, bool isUserGenerated)
    {
        lock (_lock) {
            if (_fadeEngineTimer.Enabled) {
                _fadeBrightnessTarget = _brightness;
                _fadeEngineTimer.Enabled = false;
                if(isUserGenerated)
                    return;
            }

            if(mode == Controller.TurnOnOffModes.TurnOn && IsOn)
                return;

            if(mode == Controller.TurnOnOffModes.TurnOff && !IsOn)
                return;

            var newBrightness = _lastSteadyStateBrightness;
            if (IsOn) {
                _lastSteadyStateBrightness = _brightness;
                newBrightness = 0;
            }
            _fadeBrightnessTarget = newBrightness;
            var transitionTime = CalcTransitionTime(newBrightness, SlowTransitionTime);
            SetBrightness(newBrightness, transitionTime);
        }
    }

    private double CalcTransitionTime(double newBrightness, double maxTransitionTime) => Math.Abs(newBrightness - _brightness) * maxTransitionTime;

    internal void HandleRotateToFade(bool isUp, double degrees)
    {
        var seconds = degrees / 360.0 * FadeSecondsPerRotation;
        lock(_lock) {
            if(isUp)
                _upcomingFadeTime += seconds;
            else
                _upcomingFadeTime -= seconds;

            _fadeTriggerTimer.Stop();
            _fadeTriggerTimer.Start();
        }
    }

    private void TriggerFade(object? sender, ElapsedEventArgs args)
    {
        lock(_lock) {
            _consoleOutput.InfoLine($"Trigger fade: {_upcomingFadeTime}s");
            var directionIsUp = _upcomingFadeTime > 0;
            _fadeBrightnessTarget = directionIsUp ? 1.0 : 0.0;
            _fadeTime = (int) Math.Round(Math.Abs(_upcomingFadeTime)); // Unit: s
            _upcomingFadeTime = 0;
            RunFade();
        }
    }

    private void RunFade()
    {
        lock(_lock) {
            if(FadeTargetReached)
                return;

            var isUp = _fadeBrightnessTarget > _brightness;

            var (intervalMs, nextBrightness) = CalculateIntervalAndNextBrightness(isUp);
            SetBrightness(nextBrightness, intervalMs / 1000);

            if (FadeTargetReached)
                return;

            _fadeEngineTimer.Interval = intervalMs;
            _fadeEngineTimer.Start();
        }
    }

    private void SetBrightness(double value, double transitionTime = FastTransitionTime)
    {
        _brightness = Math.Clamp(value, 0.0, 1.0);
        _setBrightness(_brightness, transitionTime);
    }


    private (double intervalMs, double nextBrightness) CalculateIntervalAndNextBrightness(bool isUp)
    {
        if (isUp && !IsOn) {
            return (intervalMs: 1, nextBrightness: _brightnessConverter.MinVisibleNormBrightness);
        }

        double intervalMs;
        var nextBrightness = _brightness;
        var direction = isUp ? 1 : -1;
        do {
            var nextOk = _brightnessConverter.TryCalcNextStep(ref nextBrightness, direction);
            if (!nextOk)
                break;

            UpdateResultInterval();
        } while (intervalMs < FadeMinIntervalMs);

        var willBeDone = (nextBrightness - _fadeBrightnessTarget) * direction >= 0;
        if (willBeDone)
            nextBrightness = _fadeBrightnessTarget;

        UpdateResultInterval();

        return (intervalMs, nextBrightness);

        void UpdateResultInterval()
        {
            var delta = Math.Abs(nextBrightness - _brightness);
            intervalMs = delta * _fadeTime * 1000;
        }
    }

    internal void FadeToBrightness(double target, int duration)
    {
        lock(_lock) {
            if(duration <= 0)
                return;

            _fadeBrightnessTarget = target;
            _fadeTime = Math.Abs(_brightness - target) * duration;
            RunFade();
        }
    }

    private bool FadeTargetReached => Math.Abs(_fadeBrightnessTarget - _brightness) < float.Epsilon;

    private bool IsOn => _brightness >= _brightnessConverter.MinVisibleNormBrightness;
}
