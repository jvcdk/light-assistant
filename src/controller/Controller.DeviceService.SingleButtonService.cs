using LightAssistant.Interfaces;

namespace LightAssistant.Controller;

internal partial class Controller
{
    private abstract partial class DeviceService
    {
        internal class SingleButtonService: DeviceService
        {
            internal SingleButtonService(IDevice device, string path, string singlePush, string doublePush, string longPush,
                IConsoleOutput consoleOutput) : base(path, device, consoleOutput)
            {
                SinglePush = new PushService("Single", device, consoleOutput) { Push = singlePush};
                DoublePush = new PushService("Double", device, consoleOutput) { Push = doublePush};
                LongPush = new PushService("Long", device, consoleOutput) { Push = longPush};
            }

            public PushService SinglePush { get; private set; }
            public PushService DoublePush { get; private set; }
            public PushService LongPush { get; private set; }
        }
    }
}
