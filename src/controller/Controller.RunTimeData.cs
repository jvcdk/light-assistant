using Newtonsoft.Json;
using LightAssistant.Interfaces;

namespace LightAssistant.Controller;

internal partial class Controller
{
    public class RunTimeData
    {
        public Dictionary<string, List<Route>> Routes { get; set; } = [];
        public Dictionary<string, List<SerializableDeviceScheduleEntry>> Schedules { get; set; } = [];

        // Deafult constructor; used for de-serialization.
        public RunTimeData() { }

        internal RunTimeData(Dictionary<string, List<EventRoute>> routes, Dictionary<string, List<DeviceScheduleEntry>> schedules)
        {
            Routes = routes.ToDictionary(
                entry => entry.Key,
                entry => entry.Value.Select(route => new Route(route)).ToList()
            );
            Schedules = schedules.ToDictionary(
                entry => entry.Key,
                entry => entry.Value.Select(entry => new SerializableDeviceScheduleEntry(entry)).ToList()
            );
        }

        internal static RunTimeData? LoadFromFile(string filePath)
        {
            var data = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<RunTimeData>(data);
        }

        internal async Task SaveToFile(string filePath)
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            await File.WriteAllTextAsync(filePath, json);
        }

        internal void PopulateRoutes(Dictionary<string, List<EventRoute>> routes)
        {
            routes.Clear();
            foreach(var (key, value) in Routes)
                routes[key] = value.Select(route => new EventRoute(route)).ToList();
        }

        internal void PopulateSchedule(Dictionary<string, List<DeviceScheduleEntry>> schedules)
        {
            schedules.Clear();
            foreach(var (key, value) in Schedules)
                schedules[key] = value.Select(entry => new DeviceScheduleEntry(entry)).ToList();
        }

        public class Route(string sourceEvent, string targetAddress, string targetFunctionality) : IEventRoute
        {
            public Route(IEventRoute source) : this(source.SourceEvent, source.TargetAddress, source.TargetFunctionality) { }
            public Route() : this("", "", "") { }

            public string SourceEvent { get; set; } = sourceEvent;
            public string TargetAddress { get; set; } = targetAddress;
            public string TargetFunctionality { get; set; } = targetFunctionality;
        }

        public class SerializableDeviceScheduleEntry(string eventType, IReadOnlyDictionary<string, string> parameters, IScheduleTrigger trigger) : IDeviceScheduleEntry
        {
            public SerializableDeviceScheduleEntry(IDeviceScheduleEntry source) : this(source.EventType, source.Parameters, source.Trigger) { }
            public SerializableDeviceScheduleEntry() : this("", new Dictionary<string, string>(), new SerializableScheduleTrigger()) { }

            public string EventType { get; set; } = eventType;
            public IReadOnlyDictionary<string, string> Parameters { get; set; } = parameters;
            public IScheduleTrigger Trigger { get; set; } = trigger;
        }

        public class SerializableScheduleTrigger(IReadOnlySet<int> days, ITimeOfDay time) : IScheduleTrigger
        {
            public SerializableScheduleTrigger() : this(new HashSet<int>(), new SerializableTimeOfDay()) { }

            public IReadOnlySet<int> Days { get; set; } = days;
            public ITimeOfDay Time { get; set; } = time;
        }

        public class SerializableTimeOfDay(int hour, int minute) : ITimeOfDay
        {
            public SerializableTimeOfDay() : this(0, 0) { }
            public int Hour { get; set; } = hour;
            public int Minute { get; set; } = minute;
        }
    }
}
