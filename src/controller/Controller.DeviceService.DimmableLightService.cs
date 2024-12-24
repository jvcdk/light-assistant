using System.Diagnostics;
using LightAssistant.Interfaces;

namespace LightAssistant.Controller;

internal partial class Controller
{
    private abstract partial class DeviceService
    {
        internal class DimmableLightService : DeviceService
        {
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
                _lightFadeEngine = new LightFadeEngine(SetBrightness, _brightnessConverter, consoleOutput);
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

            private void SetBrightness(double brightness, double duration)
            {
                var rawBrightness = _brightnessConverter.NormToRaw(brightness);
                Device.SendBrightnessTransition(rawBrightness, duration);
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
        }
    }
}
