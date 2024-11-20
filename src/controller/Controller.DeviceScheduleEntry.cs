using LightAssistant.Interfaces;

namespace LightAssistant.Controller;

internal partial class Controller
{
    internal class DeviceScheduleEntry(IDeviceScheduleEntry entry) : IDeviceScheduleEntry
    {
        public string EventType { get; } = entry.EventType;
        public IReadOnlyDictionary<string, string> Parameters { get; } = entry.Parameters;
        private readonly ScheduleTrigger _trigger = new(entry.Trigger);
        public IScheduleTrigger Trigger => _trigger;

        public bool Validate(IReadOnlyList<IConsumableAction> eligibleActions) {
            var action = eligibleActions.FirstOrDefault(action => action.Type == EventType);
            if(action == null)
                return false;

            return ValidateParameters(action) &&
                _trigger.Validate();
        }

        private bool ValidateParameters(IConsumableAction action) {
            if(Parameters.Count != action.Parameters.Count)
                return false;

            foreach(var (key, value) in Parameters) {
                var param = action.Parameters.FirstOrDefault(param => param.Name == key);
                if(param == null)
                    return false;

                if(!param.Param.Validate(value))
                    return false;
            }

            return true;
        }

        public override string ToString() => $"Schedule: {EventType} with {string.Join(", ", Parameters.Select(kv => $"{kv.Key}={kv.Value}"))}, {_trigger}";
    }

    internal class ScheduleTrigger(IScheduleTrigger trigger) : IScheduleTrigger
    {
        public IReadOnlySet<int> Days { get; } = trigger.Days;
        private readonly TimeOfDay _time = new(trigger.Time);
        public ITimeOfDay Time => _time;

        public bool Validate() {
            return Days.Count <= 7 && Days.All(day => day >= 0 && day < 7) && _time.Validate();
        }

        public override string ToString() => $"Trigger: {string.Join(", ", Days)} at {Time}";
    }

    internal class TimeOfDay(ITimeOfDay time) : ITimeOfDay
    {
        public int Hour { get; } = time.Hour;
        public int Minute { get; } = time.Minute;

        public bool Validate() {
            return Hour >= 0 && Hour < 24 && Minute >= 0 && Minute < 60;
        }

        public override string ToString() => $"{Hour:D2}:{Minute:D2}";
    }
}
