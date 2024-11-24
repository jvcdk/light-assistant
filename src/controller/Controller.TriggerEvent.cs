using System.Reflection;

namespace LightAssistant.Controller;

internal partial class Controller
{
    private abstract class ActionEvent()
    {
        public bool UserGenerated { get; init; } = false;

        public string Type {
            get {
                var typeName = GetType().ToString();
                var fullName = typeof(ActionEvent).FullName ?? "";
                fullName += "_";
                if(typeName.StartsWith(fullName))
                    typeName = typeName[fullName.Length..];
                return typeName;
            }
        }
    }

    public enum TurnOnOffModes { Toggle, TurnOn, TurnOff }

    private class ActionEvent_TurnOnOff() : ActionEvent
    {
        [ParamEnum(typeof(TurnOnOffModes))]
        public TurnOnOffModes Mode { get; init; } = TurnOnOffModes.Toggle;
    }

    private class ActionEvent_FadeToBrightness() : ActionEvent
    {
        [ParamBrightness()]
        public double Brightness { get; init; }

        [ParamInt(1, 120, "s")]
        public int Duration { get; init; } = 1;
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ActionSink(string name) : Attribute
    {
        public string Name { get; } = name;
    }

    private class ActionInfo(string name, DeviceService service, MethodInfo method, Type actionType, IReadOnlyList<ParamInfo> @params)
    {
        public string Name { get; } = name;
        public DeviceService ServiceInstance { get; } = service;
        public MethodInfo Method { get; } = method;
        public IReadOnlyList<ParamInfo> Params { get; } = @params;
        public Type ActionType { get; } = actionType;
    }
}
