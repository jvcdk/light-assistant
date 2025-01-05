using System.Diagnostics;
using LightAssistant.Interfaces;
using LightAssistant.Utils;

namespace LightAssistant.Controller;

internal partial class Controller
{
    private abstract partial class DeviceService
    {
        internal class SmartPlugService(IDevice device, IConsoleOutput consoleOutput) : DeviceService("", device, consoleOutput)
        {
            private readonly SlimReadWriteDataGuard<Data> _data = new(new Data());

            internal override IEnumerable<InternalEventSink> ConsumedEvents => [
                new InternalEventSink(typeof(InternalEvent_Push), "Toggle on/off", HandleToggleOnOff),
            ];

            [ParamEnum(typeof(PowerOutageMemoryMode))]
            internal PowerOutageMemoryMode PowerOutageMemory {
                get {
                    using var _ = _data.ObtainReadLock(out var data);
                    return data.PowerOutageMemory;
                }
                set {
                    using(var _ = _data.ObtainWriteLock(out var data)) {
                        data.PowerOutageMemory = value;
                    }

                    var valueStr = NameValueAttribute.GetValue(value);
                    var msg = new Dictionary<string, string> {
                        { "power_outage_memory", valueStr }
                    };
                    Device.SendCommand(msg);
                }
            }

            [ParamEnum(typeof(IndicatorMode))]
            internal IndicatorMode Indicator {
                get {
                    using var _ = _data.ObtainReadLock(out var data);
                    return data.IndicatorMode;
                }
                set {
                    using(var _ = _data.ObtainWriteLock(out var data)) {
                        data.IndicatorMode = value;
                    }

                    var valueStr = NameValueAttribute.GetValue(value);
                    var msg = new Dictionary<string, string> {
                        { "indicator_mode", valueStr }
                    };
                    Device.SendCommand(msg);
                }
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
            private void HandleTurnOnOff(ActionEvent_TurnOnOff ev) {
                bool state;
                using(var _ = _data.ObtainWriteLock(out var data)) {
                    state = ev.Mode switch {
                        TurnOnOffModes.Toggle => !data.State,
                        TurnOnOffModes.TurnOn => true,
                        TurnOnOffModes.TurnOff => false,
                        _ => throw new ArgumentOutOfRangeException(nameof(ev.Mode))
                    };
                    data.State = state;
                }
                Device.SendStateChange(state);
            }

            private class Data
            {
                public bool State;
                public PowerOutageMemoryMode PowerOutageMemory;
                public IndicatorMode IndicatorMode;
            }

            internal enum PowerOutageMemoryMode { Off, On, Restore }

            internal enum IndicatorMode {
                [NameValue("Off", "off")]
                Off,

                [NameValue("On", "on")]
                On,

                [NameValue("Normal", "off/on")]
                OffOn,

                [NameValue("Inverse", "on/off")]
                OnOff
            }
        }
    }
}
