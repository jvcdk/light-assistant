using LightAssistant.Interfaces;

namespace LightAssistant.Controller;

internal partial class Controller
{
    private class DeviceStatus : IDeviceStatus
    {
        private int? _linkQuality;
        public int? LinkQuality => _linkQuality;

        private int? _battery;
        public int? Battery => _battery;

        private int? _brightness;
        public int? Brightness => _brightness;

        private bool? _state;
        public bool? State => _state;

        private bool UpdateFrom(Dictionary<string, string> source, string propName, ref int? dst)
        {
            if(source.TryGetValue(propName, out var str) &&
                int.TryParse(str, out var value) &&
                value != dst) {
                    dst = value;
                    return true;
                }

            return false;
        }

        private bool UpdateFrom(Dictionary<string, string> source, string propName, ref bool? dst)
        {
            if(source.TryGetValue(propName, out var str)) {
                str = str.ToLower();
                switch(str) {
                    case "1":
                    case "yes":
                    case "true":
                    case "on":
                        dst = true;
                        return true;

                    case "0":
                    case "no":
                    case "false":
                    case "off":
                        dst = false;
                        return true;
                }
            }

            return false;
        }

        internal bool UpdateFrom(Dictionary<string, string> source)
        {
            var result = UpdateFrom(source, "linkquality", ref _linkQuality);
            result = UpdateFrom(source, "battery", ref _battery) || result;
            result = UpdateFrom(source, "brightness", ref _brightness) || result;
            result = UpdateFrom(source, "state", ref _state) || result;

            return result;                
        }
    }
}
