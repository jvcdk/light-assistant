using System.Reflection;

namespace LightAssistant.Controller;

internal partial class Controller
{
    private abstract class ActionEvent(string targetDevice)
    {
        public string TargetDevice { get; } = targetDevice;

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

    private class ActionEvent_TurnOnOff(string targetDevice, TurnOnOffModes mode) : ActionEvent(targetDevice)
    {
        [ParamEnum(typeof(TurnOnOffModes))]
        public TurnOnOffModes Mode { get; } = mode;
    }

    private class ActionEvent_FadeToBrightness(string targetDevice, double brightness, double duration) : ActionEvent(targetDevice)
    {
        [ParamBrightness()]
        public double Brightness { get; } = brightness;

        [ParamFloat(0, 120)]
        public double Duration { get; } = duration;
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ActionSink(string name) : Attribute
    {
        public string Name { get; } = name;
    }

    private class ActionInfo(string name, MethodInfo method, IReadOnlyList<ParamInfo> @params)
    {
        public string Name { get; } = name;
        public MethodInfo Method { get; } = method;
        public IReadOnlyList<ParamInfo> Params { get; } = @params;
    }
}
