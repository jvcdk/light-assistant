
namespace LightAssistant.Interfaces
{
    internal interface IDeviceScheduleEntry
    {
        public int Key { get; }
        public string EventType { get; }
        public IReadOnlyDictionary<string, string> Parameters { get; }
        public IScheduleTrigger Trigger { get; }
    }

    internal interface IScheduleTrigger
    {
        public IReadOnlySet<int> Days { get; }
        public ITimeOfDay Time { get; }
    }

    internal interface ITimeOfDay
    {
        public int Hour { get; }
        public int Minute { get; }
    }
}
