using LightAssistant.Interfaces;
using Newtonsoft.Json;

namespace LightAssistant.Clients;

internal partial class PiPwmClient
{
    public class Device : IDevice
    {
        public string Vendor => "TallDane";
        public string Model => "Pi5";
        public string Description => "Pi5 PWM Driver for LEDs.";
        public bool BatteryPowered => false;

        private string _name = "";
        public string Name {
            get => _name;
            set => _name = value ?? "";
        }

        private string _address = "";
        public string Address {
            get => _address;
            set => _address = value ?? "";
        }

        bool IDevice.Equals(IDevice other)
        {
            return
                Name == other.Name &&
                Address == other.Address;
        }

        internal Func<string, Dictionary<string, string>, Task>? SendToBus { get; set; }

        public void SendBrightnessTransition(int brightness, double transitionTime)
        {
            var data = new Dictionary<string, string> {
                ["transition"] = transitionTime.ToString(), 
                ["brightness"] = brightness.ToString()
            };
            SendToBus?.Invoke($"{Address}/set", data);
        }

        public Task SetName(string name)
        {
            var data = new Dictionary<string, string> {
                ["to"] = name
            };
            return SendToBus?.Invoke($"{Address}/rename", data) ?? Task.CompletedTask;
        }
    }

    public class GenericResponse
    {
        private Dictionary<string, string> _data = [];
        public Dictionary<string, string> Data {
            get => _data;
            set => _data = value ?? [];
        }

        private string _status = "";
        public string Status {
            get => _status;
            set => _status = value ?? "";
        }
    }
}
