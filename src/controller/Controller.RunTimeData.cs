using Newtonsoft.Json;
using LightAssistant.Interfaces;

namespace LightAssistant.Controller;

internal partial class Controller
{
    public class RunTimeData
    {
        public Dictionary<string, List<Route>> Routes { get; set; } = [];

        // Deafult constructor; used for de-serialization.
        public RunTimeData() { }

        internal RunTimeData(Dictionary<string, List<EventRoute>> routes, Dictionary<string, List<DeviceScheduleEntry>> _schedules)
        {
            Routes = routes.ToDictionary(
                entry => entry.Key,
                entry => entry.Value.Select(route => new Route(route.SourceEvent, route.TargetAddress, route.TargetFunctionality)).ToList()
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

        public class Route(string sourceEvent, string targetAddress, string targetFunctionality) : IEventRoute
        {
            public Route() : this("", "", "") { }

            public string SourceEvent { get; set; } = sourceEvent;
            public string TargetAddress { get; set; } = targetAddress;
            public string TargetFunctionality { get; set; } = targetFunctionality;
        }

        internal void PopulateRoutes(Dictionary<string, List<EventRoute>> routes)
        {
            routes.Clear();
            foreach(var (key, value) in Routes)
                routes[key] = value.Select(route => new EventRoute(route.SourceEvent, route.TargetAddress, route.TargetFunctionality)).ToList();
        }
    }
}
