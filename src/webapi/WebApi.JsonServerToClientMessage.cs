using System.Collections.ObjectModel;
using LightAssistant.Interfaces;
using Newtonsoft.Json;
using static System.Text.Encoding;

namespace LightAssistant.WebApi;

internal partial class WebApi
{
    public class JsonServerToClientMessage
    {
        internal byte[] Serialize() => UTF8.GetBytes(JsonConvert.SerializeObject(this));

        public IReadOnlyList<JsonDevice>? Devices { get; private set; }
        public JsonDeviceStatus? DeviceStatus { get; private set; }
        public JsonDeviceRouting? Routing { get; private set; }
        public JsonDeviceRoutingOptions? RoutingOptions { get; private set; }
        public JsonDeviceSchedule? Schedule { get; private set; }
        public JsonScheduleActionOptions? ScheduleActionOptions { get; private set; }
        public JsonOpenNetworkStatus? OpenNetworkStatus { get; private set; }

        internal static JsonServerToClientMessage Empty() => new();

        internal JsonServerToClientMessage WithDeviceList(IReadOnlyList<IDevice> devices) {
            Devices = devices.Select(JsonDevice.FromIDevice).ToList();
            return this;
        }

        internal JsonServerToClientMessage WithDeviceStatus(string address, IDeviceStatus status) {
            DeviceStatus = new JsonDeviceStatus(address, status);
            return this;
        }

        internal JsonServerToClientMessage WithDeviceRouting(string address, IReadOnlyList<JsonDeviceRoute> routing) {
            Routing = new JsonDeviceRouting(address, routing);
            return this;
        }

        internal JsonServerToClientMessage WithDeviceRoutingOptions(string address, IReadOnlyList<IProvidedEvent> providedEvents, IReadOnlyList<IConsumableEvent> consumableEvents) {
            var jsonProvidedEvents = providedEvents.Select(ev => new JsonDeviceProvidedEvent(ev.Type, ev.Name)).ToList();
            var jsonConsumableEvents = consumableEvents.Select(ev => new JsonDeviceConsumableEvent(ev.Type, ev.Functionality)).ToList(); 

            RoutingOptions = new JsonDeviceRoutingOptions(address, jsonProvidedEvents, jsonConsumableEvents);
            return this;
        }

        internal JsonServerToClientMessage WithScheduleActionOptions(string address, IReadOnlyList<IConsumableAction> consumableActions)
        {
            var jsonConsumableActions = consumableActions.Select(entry => {
                var @params = entry.Parameters.Select(param => JSonParamInfo.FromParamInfo(param)).ToList();
                return new JsonDeviceConsumableAction(entry.Type, @params);
            }).ToList();
            ScheduleActionOptions = new JsonScheduleActionOptions(address, jsonConsumableActions);
            return this;
        }

        internal JsonServerToClientMessage WithDeviceSchedule(string address, IReadOnlyList<JsonDeviceScheduleEntry> schedule)
        {
            Schedule = new JsonDeviceSchedule(address, schedule);
            return this;
        }

        internal JsonServerToClientMessage WithOpenNetworkStatus(bool status, int time) {
            OpenNetworkStatus = new JsonOpenNetworkStatus(status, time);
            return this;
        }
    }

    /// <summary>
    /// Serializing an interface (i.e. IDevice) directly, does not produce json information for the interface,
    /// but for the implementing object. Hence we need to create a specific object for IDevice.
    /// 
    /// Must match IDevice in JsonTypes.tsx
    /// </summary>
    public class JsonDevice(IDevice source)
    {
        public string Name { get; } = source.Name;
        public string Address { get; } = source.Address;
        public string Vendor { get; } = source.Vendor;
        public string Model { get; } = source.Model;
        public string Description { get; } = source.Description;
        public bool BatteryPowered { get; } = source.BatteryPowered;

        public static JsonDevice FromIDevice(IDevice source) => new(source);
    }

    /// <summary>
    /// Must match IDeviceStatus in JsonTypes.tsx
    /// </summary>
    public class JsonDeviceStatus(string address, IDeviceStatus source)
    {
        public string Address { get; } = address;
        public int? LinkQuality { get; } = source.LinkQuality;
        public int? Battery { get; } = source.Battery;
        public int? Brightness { get; } = source.Brightness;
        public bool? State { get; } = source.State;
    }

    /// <summary>
    /// Must match IDeviceRouting in JsonTypes.tsx
    /// </summary>
    public class JsonDeviceRouting(string address, IReadOnlyList<JsonDeviceRoute> routing)
    {
        public string Address { get; } = address;
        public IReadOnlyList<JsonDeviceRoute> Routing = routing;
    }

    public class JsonDeviceRoute(string sourceEvent, string targetAddress, string targetFunctionality) : IEventRoute
    {
        public string SourceEvent { get; } = sourceEvent;
        public string TargetAddress { get; } = targetAddress;
        public string TargetFunctionality { get; } = targetFunctionality;
    }

    public class JsonDeviceRoutingOptions(string address, IReadOnlyList<JsonDeviceProvidedEvent> providedEvents, IReadOnlyList<JsonDeviceConsumableEvent> consumedEvents)
    {
        public string Address { get; } = address;
        public IReadOnlyList<JsonDeviceProvidedEvent> ProvidedEvents { get; } = providedEvents;
        public IReadOnlyList<JsonDeviceConsumableEvent> ConsumableEvents { get; } = consumedEvents;
    }

    public class JsonScheduleActionOptions(string address, IReadOnlyList<JsonDeviceConsumableAction> consumableActions)
    {
        public string Address { get; } = address;
        public IReadOnlyList<JsonDeviceConsumableAction> ConsumableActions { get; } = consumableActions;
    }

    internal class JsonDeviceConsumableEvent(string eventType, string targetName)
    {
        public string EventType { get; } = eventType;
        public string TargetName { get; } = targetName;
    }

    internal class JsonDeviceProvidedEvent(string eventType, string name)
    {
        public string EventType { get; } = eventType;
        public string Name { get; } = name;
    }

    internal class JsonDeviceConsumableAction(string eventType, IReadOnlyList<JSonParamInfo> parameters)
    {
        public string EventType { get; } = eventType;
        public IReadOnlyList<JSonParamInfo> Parameters { get; } = parameters;
    }

    /**
     * JSonParamInfo covers both ParamInfo and ParamDescriptor.
     * This is a base class; descendants match descendants of ParamDescriptor.
     */
    internal abstract class JSonParamInfo(string name, string units)
    {
        public abstract string Type { get; }
        public string Name { get; } = name;
        public string Units { get; } = units;

        public static JSonParamInfo FromParamInfo(ParamInfo source) {
            var param = source.Param;
            return source.Param switch {
                ParamEnum paramEnum => new JSonParamEnum(source.Name, paramEnum.Values, paramEnum.Default, param.Units),
                ParamBrightness paramBrightness => new JsonParamBrightness(source.Name, paramBrightness.Min, paramBrightness.Max, paramBrightness.Default),
                ParamFloat paramFloat => new JSonParamFloat(source.Name, paramFloat.Min, paramFloat.Max, paramFloat.Default, param.Units),
                ParamInt paramInt => new JSonParamInt(source.Name, paramInt.Min, paramInt.Max, paramInt.Default, param.Units),
                _ => throw new ArgumentException("Unknown ParamDescriptor type"),
            };
        }
    }

    internal class JSonParamEnum(string name, string[] values, string defaultValue, string units) : JSonParamInfo(name, units)
    {
        public override string Type => "enum";
        public string[] Values { get; } = values;
        public string Default { get; } = defaultValue;
    }

    internal class JSonParamFloat(string name, double min, double max, double defaultValue, string units) : JSonParamInfo(name, units)
    {
        public override string Type => "float";
        public double Min { get; } = min;
        public double Max { get; } = max;
        public double Default { get; } = defaultValue;
    }

    internal class JsonParamBrightness(string name, double min, double max, double defaultValue) : JSonParamFloat(name, min, max, defaultValue, "")
    {
        public override string Type => "brightness";
    }

    internal class JSonParamInt(string name, int min, int max, int defaultValue, string units) : JSonParamInfo(name, units)
    {
        public override string Type => "int";
        public int Min { get; } = min;
        public int Max { get; } = max;
        public int Default { get; } = defaultValue;
    }

    internal class JsonOpenNetworkStatus(bool status, int time)
    {
        public bool Status { get; } = status;
        public int Time { get; } = time;
    }

    internal class JsonDeviceSchedule(string address, IReadOnlyList<JsonDeviceScheduleEntry> schedule)
    {
        public string Address { get; } = address;
        public IReadOnlyList<JsonDeviceScheduleEntry> Schedule = schedule;
    }

    internal class JsonDeviceScheduleEntry(string eventType, IReadOnlyDictionary<string, string> parameters, IScheduleTrigger trigger) : IDeviceScheduleEntry
    {
        public JsonDeviceScheduleEntry() : this("", new Dictionary<string, string>(), new JsonScheduleTrigger()) { }

        public string EventType { get; set; } = eventType;
        public IReadOnlyDictionary<string, string> Parameters { get; set; } = parameters;
        public IScheduleTrigger Trigger { get; set; } = trigger;
    }

    internal class JsonScheduleTrigger(IReadOnlySet<int> days, ITimeOfDay time) : IScheduleTrigger
    {
        public JsonScheduleTrigger() : this(new HashSet<int>(), new JsonTimeOfDay()) { }

        public IReadOnlySet<int> Days { get; set; } = days;
        public ITimeOfDay Time { get; set; } = time;
    }

    internal class JsonTimeOfDay(int hour, int minute) : ITimeOfDay
    {
        public JsonTimeOfDay() : this(0, 0) { }

        public int Hour { get; set; } = hour;
        public int Minute { get; set; } = minute;
    }
}
