namespace unittest.Controller.DeviceService;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LightAssistant;
using LightAssistant.Controller;
using LightAssistant.Interfaces;
using NSubstitute;

public class DimmableLightServiceTests
{
    private Controller _controller;
    private TestDeviceBus _deviceBus = new();

    private MockPi5 _pi5 = new(); // We are using this to get to DimmableLightService
    private IDevice _weLink = CreateEWeLinkWB01(); // We are using this to get to SingleButtonService

    [SetUp]
    public void Setup()
    {
        // Set up controller
        var consoleOutput = new ConsoleOutput() { Verbose = false };
        var deviceBuses = new List<IDeviceBus>() { _deviceBus };
        var gui = Substitute.For<IUserInterface>();
        var storage = Substitute.For<Controller.IDataStorage>();
        storage.LoadData().Returns(new Controller.RunTimeData());
        _controller = new Controller(consoleOutput, deviceBuses, gui, storage, 1);

        _deviceBus.DiscoverDevice(_pi5);
        _deviceBus.DiscoverDevice(_weLink);
    }

    private void ConfigureRoute(IDevice src, string srcEvent, IDevice dst, string dstFunc)
    {
        var route = Substitute.For<IEventRoute>();
        route.SourceEvent.Returns(srcEvent);
        route.TargetAddress.Returns(dst.Address);
        route.TargetFunctionality.Returns(dstFunc);
        _controller.SetDeviceOptions(src.Address, src.Name, [route], [], []).Wait();
    }

    private static IDevice CreateEWeLinkWB01()
    {
        var device = Substitute.For<IDevice>();
        device.Address.Returns("AA:BB:CC:DD:EE:FF");
        device.Vendor.Returns("eWeLink");
        device.Model.Returns("WB01");
        device.Name.Returns("Mock eWeLink WB01");
        device.Description.Returns("Mock eWeLink WB01 device");
        device.BatteryPowered.Returns(false);
        return device;
    }

    [Test]
    public void WhenReceivingToggleOnOff_ThenLightShouldToggle()
    {
        // Create route from eWeLink single push to Pi5 toggle
        ConfigureRoute(_weLink, "Single", _pi5, "Toggle on/off");

        _deviceBus.InvokeAction(_weLink, "single");
        Assert.That(_pi5.SendBrightnessTransitionCalls, Is.EqualTo(1));
        Assert.That(_pi5.LastBrightness, Is.EqualTo(MockPi5.MaxRawBrightness));

        _deviceBus.InvokeAction(_weLink, "single");
        Assert.That(_pi5.SendBrightnessTransitionCalls, Is.EqualTo(2));
        Assert.That(_pi5.LastBrightness, Is.EqualTo(0));

        _deviceBus.InvokeAction(_weLink, "single");
        Assert.That(_pi5.SendBrightnessTransitionCalls, Is.EqualTo(3));
        Assert.That(_pi5.LastBrightness, Is.EqualTo(MockPi5.MaxRawBrightness));
    }

    [Test]
    public void GivenDeviceWithDimmableLightService_WhenRequestingAvailableActions_ThenShouldReturnDimmableLightActions()
    {
        var actions = _controller.GetConsumableActionsFor(_pi5);
        var actionNames = actions.Select(action => action.Type).ToList();

        Assert.That(actionNames, Does.Contain("Turn on/off"));
        Assert.That(actionNames, Does.Contain("Fade to brightness"));
    }

    private class TestDeviceBus : IDeviceBus
    {
        public event Action<IDevice> DeviceDiscovered = delegate { };
        public event Action<IDevice> DeviceUpdated = delegate { };
        public event Action<IDevice, Dictionary<string, string>> DeviceAction = delegate { };
        public event Action<bool, int> NetworkOpenStatus = delegate { };

        public Task Connect() => Task.CompletedTask; // Do nothing
        public Task RequestOpenNetwork(int openNetworkTimeSeconds) => Task.CompletedTask; // Do nothing

        internal void DiscoverDevice(IDevice device) => DeviceDiscovered.Invoke(device);

        internal void InvokeAction(IDevice device, string action) =>
            DeviceAction.Invoke(device, new Dictionary<string, string> { { "action", action } });
    }

    private class MockPi5 : IDevice
    {
        internal static int MaxRawBrightness => (1 << 15) - 1; // From Controller.DeviceServiceMapping.PiPwm
        public string Name { get; private set; } = "Mock Pi5";
        public string Address => "00:11:22:33:44:55";
        public string Vendor => "TallDane";
        public string Model => "Pi5";
        public string Description => "Mock TallDane Pi5 device";
        public bool BatteryPowered => false;

        public bool Equals(IDevice other) =>
            Name == other.Name &&
            Address == other.Address &&
            Vendor == other.Vendor &&
            Model == other.Model &&
            Description == other.Description &&
            BatteryPowered == other.BatteryPowered;

        public Task SendBrightnessTransition(int brightness, double transitionTime)
        {
            SendBrightnessTransitionCalls++;
            LastBrightness = brightness;
            LastTransitionTime = transitionTime;
            return Task.CompletedTask;
        }
        internal int SendBrightnessTransitionCalls = 0;
        internal int LastBrightness = -1;
        internal double LastTransitionTime = -1;

        public Task SendColorTempTransition(int colorTempRaw, double transitionTime) => Task.CompletedTask; // Do nothing

        public Task SendCommand(Dictionary<string, string> data) => Task.CompletedTask; // Do nothing

        public Task SendStateChange(bool state) => Task.CompletedTask; // Do nothing

        public Task SetName(string name)
        {
            Name = name;
            return Task.CompletedTask;
        }
    }
}
