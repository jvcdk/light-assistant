using System.Reflection;
using LightAssistant.Interfaces;
using LightAssistant.Utils;

namespace LightAssistant.Controller;

internal partial class Controller
{
    private abstract class DeviceServiceCollection(IConsoleOutput consoleOutput)
    {
        private readonly IConsoleOutput _consoleOutput = consoleOutput;

        internal IEnumerable<InternalEvent> ProcessExternalEvent(IDevice sourceDevice, IReadOnlyDictionary<string, string> data)
        {
            foreach(var service in EnumerateServices()) {
                foreach(var ev in service.ProcessExternalEvent(sourceDevice, data))
                    yield return ev;
            }
        }

        internal void ProcessInternalEvent(InternalEvent ev, string targetFunctionality)
        {
            foreach(var service in EnumerateServices())
                service.ProcessInternalEvent(ev, targetFunctionality);
        }

        internal void ProcessScheduleAction(string eventType, IReadOnlyDictionary<string, string> parameters)
        {
            foreach(var action in ConsumedActions) {
                if(action.Name == eventType) {
                    var param = CreateActionParam(action.ActionType, parameters);
                    if(param != null)
                        action.Method.Invoke(action.ServiceInstance, [param]);
                }
            }
        }

        private object? CreateActionParam(Type actionType, IReadOnlyDictionary<string, string> parameters)
        {
            try {
                var result = Activator.CreateInstance(actionType);
                if(result == null) {
                    _consoleOutput.ErrorLine($"Failed to create instance action parameter of type {actionType}.");
                    return null;
                }

                foreach(var prop in actionType.GetProperties()) {
                    if(parameters.TryGetValue(prop.Name, out var value)) {
                        var convertedValue = ChangeType(value, prop.PropertyType, _consoleOutput);
                        if(convertedValue != null)
                            prop.SetValue(result, convertedValue);
                    }
                }

                return result;
            }
            catch(Exception ex) {
                _consoleOutput.ErrorLine($"Failed to create action parameter of type {actionType}. Msg.: " + ex.Message);
                return null;
            }
        }

        private static object? ChangeType(string value, Type type, IConsoleOutput consoleOutput)
        {
            try {
                if(type.IsEnum)
                    return Enum.Parse(type, value.SentenceToCamelCase());

                return Convert.ChangeType(value, type);
            }
            catch(Exception ex) {
                consoleOutput.ErrorLine($"Failed to convert value '{value}' to type {type}. Msg.: " + ex.Message);
                return null;
            }
        }

        private IEnumerable<DeviceService> EnumerateServices() => this.EnumeratePropertiesOfType<DeviceService>();

        internal IEnumerable<InternalEventSink> ConsumedEvents =>
            EnumerateServices().SelectMany(service => service.ConsumedEvents);

        internal IEnumerable<InternalEventSource> ProvidedEvents =>
            EnumerateServices().SelectMany(service => service.ProvidedEvents);

        internal IEnumerable<ActionInfo> ConsumedActions =>
            EnumerateServices().SelectMany(GetActions);
        
        internal IEnumerable<IServiceOption> ServiceOptions =>
            ServiceOptionsPrivate.Cast<IServiceOption>();

        private IEnumerable<ServiceOption> ServiceOptionsPrivate =>
            EnumerateServices().SelectMany(GetServiceOptions);

        private static IEnumerable<ActionInfo> GetActions(DeviceService service)
        {
            return service.EnumerateMethodsWithAttribute<ActionSink>()
                .Select(tuple => (tuple.method, tuple.attr, param: tuple.method.GetParameters()))
                .Where(tuple => tuple.param.Length == 1 && tuple.param[0].ParameterType.IsSubclassOf(typeof(ActionEvent)))
                .Select(tuple => {
                    var param = tuple.param[0];
                    var paramInfo = param.ParameterType
                        .EnumeratePropertiesWithAttribute<ParamDescriptor>()
                        .Select(prop => new ParamInfo(prop.prop.Name, prop.attr!))
                        .ToList();
                    return new ActionInfo(tuple.attr.Name.CamelCaseToSentence(), service, tuple.method, param.ParameterType, paramInfo);
                });
        }

        private static IEnumerable<ServiceOption> GetServiceOptions(DeviceService service)
        {
            return service.EnumeratePropertiesWithAttribute<ParamDescriptor>()
                .Select(prop => {
                    void action(string value) => SetServiceOption(service, prop.prop, value);
                    var value = prop.prop.GetValue(service);
                    if(value == null)
                        return null;
                    return new ServiceOption(prop.prop.Name.CamelCaseToSentence(), value, prop.attr!, action);
                })
                .Where(option => option != null)
                .Select(option => option!);
        }

        private static void SetServiceOption(DeviceService service, PropertyInfo prop, string value)
        {
            try {
                prop.SetValue(service, Convert.ChangeType(value, prop.PropertyType));
            }
            catch(Exception ex) {
                service.ConsoleOutput.ErrorLine($"Failed to set service option {prop.Name} to value '{value}'. Msg.: " + ex.Message);
            }
        }

        internal void PreviewDeviceOption(string value, PreviewMode previewMode)
        {
            foreach(var service in EnumerateServices().OfType<IServicePreviewOption>())
                service.PreviewDeviceOption(value, previewMode);
        }

        internal List<ServiceOptionValue> SetServiceOptionValues(IEnumerable<IServiceOptionValue> serviceOptionValues)
        {
            var existingServiceOptions = ServiceOptionsPrivate.ToList();
            foreach(var serviceOptionValue in serviceOptionValues) {
                var serviceOption = existingServiceOptions.FirstOrDefault(option => option.Param.Name == serviceOptionValue.Name);
                if(serviceOption == null)
                    continue;
                serviceOption.Action(serviceOptionValue.Value);
            }

            // Don't re-use existingServiceOptions, but instead re-iterate via ServiceOptionsPrivate, to re-fetch current values.
            return ServiceOptionsPrivate
                .Select(option => new { name = option.Param.Name, value = option.Value?.ToString() })
                .Where(option => option.value != null)
                .Select(option => new ServiceOptionValue(option.name, option.value!))
                .ToList();
        }
    }

    private class EmptyDeviceServiceCollection() : DeviceServiceCollection(new ConsoleOutput()) { }
}
