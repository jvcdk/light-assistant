using System.Diagnostics;
using LightAssistant.Interfaces;
using LightAssistant.Utils;

using Timer = System.Timers.Timer;

namespace LightAssistant.Controller;

internal partial class Controller
{
    private abstract partial class DeviceService
    {
        internal class DimmableLightService : DeviceService, IServicePreviewOption
        {
            private const double TransitionTime = 0.25; // s
            private const double PreviewTimeoutMs = 5000; // ms

            private readonly SlimReadWriteDataGuard<Data> _data = new(new Data());

            private readonly LightFadeEngine _lightFadeEngine;
            private readonly LightDimEngine _lightDimEngine = new();
            private readonly BrightnessConverter _brightnessConverter;

            internal override IEnumerable<InternalEventSink> ConsumedEvents => [
                new InternalEventSink(typeof(InternalEvent_Push), "Toggle on/off", HandleToggleOnOff),
                new InternalEventSink(typeof(InternalEvent_Rotate), "Dim", HandleDim),
                new InternalEventSink(typeof(InternalEvent_Rotate), "Fade", HandleRotateToFade)
            ];

            public DimmableLightService(IDevice device, int maxBrightness, IConsoleOutput consoleOutput) : base("", device, consoleOutput)
            {
                _brightnessConverter = new(maxBrightness);
                _lightFadeEngine = new LightFadeEngine(SetFadeBrightness, _brightnessConverter, consoleOutput);

                using(var _ = _data.ObtainWriteLock(out var data)) {
                    data.Timer.AutoReset = false;
                    data.Timer.Interval = PreviewTimeoutMs;
                    data.Timer.Elapsed += (sender, args) => ResetPreview();
                }
            }

            [ParamBrightness(PreviewMode.Raw, BrightnessConverter.MinMidBrightness, BrightnessConverter.MaxMidBrightness)]
            public double MidBrightness { 
                get => _brightnessConverter.MidBrightness;
                set => _brightnessConverter.MidBrightness = value;
            }

            [ParamBrightness(PreviewMode.Normalized, 0, 1)]
            public double MinTurnOnBrightness { 
                get => _lightFadeEngine.MinTurnOnBrightness;
                set => _lightFadeEngine.MinTurnOnBrightness = value;
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
            private void HandleTurnOnOff(ActionEvent_TurnOnOff ev) => _lightFadeEngine.TurnOnOff(ev.Mode, ev.UserGenerated);

            private void SetFadeBrightness(double brightness, double duration)
            {
                bool isPreviewMode;
                using(var _ = _data.ObtainWriteLock(out var data)) {
                    data.FadeBrightnes = brightness;
                    isPreviewMode = data.PreviewMode;
                }

                if(!isPreviewMode) {
                    var rawBrightness = _brightnessConverter.NormToRaw(brightness);
                    Device.SendBrightnessTransition(rawBrightness, duration);
                }
            }

            private void HandleDim(InternalEvent ev)
            {
                if(ev is not InternalEvent_Rotate evRotate) {
                    Debug.Assert(false);
                    return;
                }

                var step = _lightDimEngine.HandleDim(evRotate.IsUp, evRotate.Degrees);
                _lightFadeEngine.DimStep(step);
            }

            private void HandleRotateToFade(InternalEvent ev)
            {
                if(ev is not InternalEvent_Rotate evRotate) {
                    Debug.Assert(false);
                    return;
                }

                _lightFadeEngine.HandleRotateToFade(evRotate.IsUp, evRotate.Degrees);
            }

            [ActionSink("Fade to brightness")]
            private void HandleFade(ActionEvent_FadeToBrightness ev)
            {
                ConsoleOutput.InfoLine($"HandleFade: {ev.Brightness} in {ev.Duration} min");
                if(ev.Duration <= 0)
                    return;

                var duration = ev.Duration * 60; // Unit: s
                _lightFadeEngine.FadeToBrightness(ev.Brightness, duration);
            }

            public void PreviewDeviceOption(string value, PreviewMode previewMode)
            {
                var parseOk = double.TryParse(value, out var brightness);
                if(!parseOk) {
                    ConsoleOutput.ErrorLine($"Invalid brightness value: {value}");
                    brightness = 0;
                }

                var setPreview = (previewMode != PreviewMode.None) && parseOk;
                using(var _ = _data.ObtainWriteLock(out var data)) {
                    if(setPreview) {
                        int raw = previewMode == PreviewMode.Normalized ?
                            _brightnessConverter.NormToRaw(brightness) :
                            _brightnessConverter.NormToRawRaw(brightness);
                        Device.SendBrightnessTransition(raw, TransitionTime);
                        data.PreviewMode = true;
                        data.Timer.Stop();
                        data.Timer.Start();
                    }
                    else
                        ResetPreview_(data);
                }
            }

            private void ResetPreview()
            {
                using(var _ = _data.ObtainWriteLock(out var data)) {
                    if (!data.PreviewMode)
                        return;

                    ResetPreview_(data);
                }
            }

            private void ResetPreview_(Data data)
            {
                data.PreviewMode = false;
                data.Timer.Stop();
                Device.SendBrightnessTransition(_brightnessConverter.NormToRaw(data.FadeBrightnes), TransitionTime);
            }
        }

        private class Data
        {
            public double FadeBrightnes;
            public bool PreviewMode;
            public readonly Timer Timer = new();
        }
    }
}
