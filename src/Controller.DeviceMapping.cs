using LightAssistant.Interfaces;
using Newtonsoft.Json;

namespace LightAssistant;

internal partial class Controller
{
    private class DeviceMapping
    {
        private class ServiceCollection : List<DeviceService> { }
        private class ModelCollection : Dictionary<string, ServiceCollection> { }

        private readonly string _configFile;
        private readonly IConsoleOutput _consoleOutput;
        private readonly Dictionary<string, ModelCollection> _data = new();

        internal DeviceMapping(string configFile, IConsoleOutput consoleOutput)
        {
            _configFile = configFile;
            _consoleOutput = consoleOutput;
            Load(configFile);
        }

        private void Load(string filePath)
        {
            _data.Add("_TZ3000_qja6nq5z", new ModelCollection {
                {
                    "TS004F", new ServiceCollection {
                        new DeviceService.AutoModeChangeService {
                            ModeField = "operation_mode",
                            FromMode = "event",
                            ModeChangeCommand = "some command"
                        },
                        new DeviceService.SmartKnobService(
                            push: new DeviceService.PushService { Push = "toggle"},
                            rotateNormal: new DeviceService.RotateService { RotateLeft = "brightness_step_down", RotateRight = "brightness_step_up" },
                            rotatePushed: new DeviceService.RotateService { RotateLeft = "color_temperature_step_down", RotateRight = "color_temperature_step_up" }
                        )
                    }
                }
            });
            SaveToFile();
            throw new NotImplementedException("Above code is temporary");
            if(!Path.Exists(filePath))
                return;

            try {
                var json = File.ReadAllText(filePath);
                JsonConvert.PopulateObject(json, _data);
                _consoleOutput.MessageLine($"Loaded device mapping file {filePath}.");
            }
            catch(Exception ex) {
                _consoleOutput.ErrorLine($"Failed loading device mapping file {filePath}. Error: " + ex.Message);
            }
        }

        public void SaveToFile()
        {
            try {
                var json = JsonConvert.SerializeObject(_data, Formatting.Indented);
                File.WriteAllText(_configFile, json);
                _consoleOutput.MessageLine($"Saved device mapping file {_configFile}.");
            }
            catch(Exception ex) {
                    _consoleOutput.ErrorLine($"Failed saving device mapping file {_configFile}. Error: " + ex.Message);
            }
        }

        internal List<DeviceService> GetServicesFor(IDevice device)
        {
            //return [];
            throw new NotImplementedException();
        }
    }
}
