using System.Diagnostics;
using LightAssistant.Interfaces;
using LightAssistant.Utils;

namespace LightAssistant.Controller;

internal partial class Controller
{
    private abstract partial class DeviceService
    {
        internal class CctLightService : DimmableLightService
        {
            private readonly SlimReadWriteDataGuard<Data> _data = new(new Data());
            private readonly int _minColorTemp;
            private readonly int _maxColorTemp;

            internal override IEnumerable<InternalEventSink> ConsumedEvents =>
                base.ConsumedEvents.Concat([
                    new(typeof(InternalEvent_Rotate), "Color temperature", HandleAdjustColorTemp),
                    new(typeof(InternalEvent_Push), "Reset color temperature", HandleResetColorTemp),
                ]);

            public CctLightService(IDevice device, int maxBrightness, int minColorTemp, int maxColorTemp, IConsoleOutput consoleOutput) : base(device, maxBrightness, consoleOutput)
            {
                _minColorTemp = minColorTemp;
                _maxColorTemp = maxColorTemp;

                using var _ = _data.ObtainWriteLock(out var data);
                data.ColorTemp = 0.5;
            }

            private void HandleResetColorTemp(InternalEvent ev)
            {
                Debug.Assert(ev.GetType() == typeof(InternalEvent_Push));

                using(var _ = _data.ObtainWriteLock(out var data)) {
                    data.ColorTemp = 0.5;
                }
                SendColorTemp(0.5);
            }

            private void HandleAdjustColorTemp(InternalEvent ev)
            {
                if (ev is not InternalEvent_Rotate evRotate) {
                    Debug.Assert(false);
                    return;
                }

                var sign = evRotate.IsUp ? 1 : -1;
                var change = evRotate.Degrees / 360 * sign;
                double colorTemp;
                using (var _ = _data.ObtainWriteLock(out var data)) {
                    data.ColorTemp = Math.Clamp(data.ColorTemp + change, 0, 1);
                    colorTemp = data.ColorTemp;
                }

                SendColorTemp(colorTemp);
            }

            private void SendColorTemp(double colorTemp)
            {
                var colorTempRaw = _minColorTemp + (int)(colorTemp * (_maxColorTemp - _minColorTemp));
                Device.SendColorTempTransition(colorTempRaw, TransitionTime);
            }

            private class Data
            {
                public double ColorTemp;
            }
        }
    }
}
