using LightAssistant.Interfaces;

namespace LightAssistant.Controller;

internal partial class Controller
{
    internal interface IDataStorage
    {
        RunTimeData LoadData();
        Task SaveData(RunTimeData data);
    }

    public class RunTimeData
    {
        public Dictionary<string, List<Route>> Routes { get; set; } = [];
        public Dictionary<string, List<SerializableDeviceScheduleEntry>> Schedules { get; set; } = [];
        public Dictionary<string, List<ServiceOptionValue>> ServiceOptionValues { get; set; } = [];

        // Default constructor; used for de-serialization.
        public RunTimeData() { }

        internal void SetSchedules(Dictionary<string, List<DeviceScheduleEntry>> schedules)
        {
            Schedules = schedules.ToDictionary(
                entry => entry.Key,
                entry => entry.Value.Select(entry => new SerializableDeviceScheduleEntry(entry)).ToList()
            );
        }

        internal void SetRoutes(Dictionary<string, List<EventRoute>> routes)
        {
            Routes = routes.ToDictionary(
                entry => entry.Key,
                entry => entry.Value.Select(route => new Route(route)).ToList()
            );
        }

        internal void SetServiceOptionValues(Dictionary<string, IReadOnlyList<ServiceOptionValue>> serviceOptionValues)
        {
            ServiceOptionValues = serviceOptionValues.ToDictionary(
                entry => entry.Key,
                entry => entry.Value.Select(value => new ServiceOptionValue(value)).ToList()
            );
        }

        internal void PopulateRoutes(Dictionary<string, List<EventRoute>> routes)
        {
            routes.Clear();
            foreach (var (key, value) in Routes)
                routes[key] = value.Select(route => new EventRoute(route)).ToList();
        }

        internal void PopulateSchedules(Dictionary<string, List<DeviceScheduleEntry>> schedules)
        {
            schedules.Clear();
            foreach (var (key, value) in Schedules)
                schedules[key] = value.Select(entry => new DeviceScheduleEntry(entry)).ToList();
        }

        internal void PopulateServiceOptionValues(Dictionary<string, IReadOnlyList<ServiceOptionValue>> serviceOptionValues)
        {
            serviceOptionValues.Clear();
            foreach (var (key, value) in ServiceOptionValues)
                serviceOptionValues[key] = value.Select(value => new ServiceOptionValue(value)).ToList();
        }

        public class Route(string sourceEvent, string targetAddress, string targetFunctionality) : IEventRoute
        {
            public Route(IEventRoute source) : this(source.SourceEvent, source.TargetAddress, source.TargetFunctionality) { }
            public Route() : this("", "", "") { }

            public string SourceEvent { get; set; } = sourceEvent;
            public string TargetAddress { get; set; } = targetAddress;
            public string TargetFunctionality { get; set; } = targetFunctionality;
        }

        public class SerializableDeviceScheduleEntry(int key, string eventType, IReadOnlyDictionary<string, string> parameters, IScheduleTrigger trigger) : IDeviceScheduleEntry
        {
            public SerializableDeviceScheduleEntry(IDeviceScheduleEntry source) : this(source.Key, source.EventType, source.Parameters, source.Trigger) { }
            public SerializableDeviceScheduleEntry() : this(0, "", new Dictionary<string, string>(), new SerializableScheduleTrigger()) { }

            public int Key { get; set; } = key;
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
