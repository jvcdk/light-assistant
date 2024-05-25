using System.Diagnostics;
using EmbedIO;
using EmbedIO.WebSockets;
using LightAssistant.Interfaces;
using LightAssistant.Utils;
using static System.Text.Encoding;

namespace LightAssistant.WebApi;

internal partial class WebApi : WebSocketModule, IDisposable, IUserInterface
{
    private readonly IConsoleOutput _consoleOutput;
    private readonly WebServer _webServer;
    private readonly string _rootUrl;

    public IController? AppController { get; set; }

    public WebApi(IConsoleOutput consoleOutput, string hostAddr, int port) : base("/", true)
    {
        _consoleOutput = consoleOutput;
        _rootUrl = $"http://{hostAddr}:{port}";

        _webServer = CreateWebServer();
    }

    public new void Dispose()
    {
        base.Dispose();
        _webServer.Dispose();
    }

    public async Task Run()
    {
        _consoleOutput.MessageLine($"Web API listening on {_rootUrl}.");
        await _webServer.RunAsync();
    }

    private WebServer CreateWebServer()
    {
        var server = new WebServer(o => o
            .WithUrlPrefix(_rootUrl)
            .WithMode(HttpListenerMode.EmbedIO)
        )
        .WithLocalSessionManager()
        .WithModule(this);

        server.StateChanged += (s, e) => _consoleOutput.InfoLine($"WebServer New State - {e.NewState}");

        return server;
    }

    protected override Task OnClientConnectedAsync(IWebSocketContext context)
    {
        if(AppController == null) {
            _consoleOutput.ErrorLine("Lost a client as we were not ready for a connection yet.");
            return Task.CompletedTask;
        }

        return DeviceListUpdated(context);
    }

    private static async Task SendMessage(IWebSocketContext context, JsonEgressMessage msg)
    {
        await context.WebSocket.SendAsync(msg.Serialize(), true);
    }

    protected override async Task OnMessageReceivedAsync(IWebSocketContext context, byte[] buffer, IWebSocketReceiveResult result)
    {
        var str = UTF8.GetString(buffer);
        if(string.IsNullOrWhiteSpace(str)) {
            _consoleOutput.ErrorLine("Message from client was empty.");
            return;
        }

        try {
            var msg = JsonIngressMessageParser.ParseMessage(str);
            if(msg == null) {
                _consoleOutput.ErrorLine("Could not parse message from client. Null object returned.");
                return;
            }

            if(msg.DeviceConfigurationChange != null)
                await HandleDeviceConfigurationChange(msg.DeviceConfigurationChange);
        }
        catch(Exception ex) {
            _consoleOutput.ErrorLine("Could not parse message from client. Error message: " + ex);
            return;
        }
    }

    private async Task HandleDeviceConfigurationChange(JsonDeviceConfigurationChange ev)
    {
        Debug.Assert(AppController != null);

        var device = AppController.GetDeviceList().FirstOrDefault(entry => entry.Address == ev.Address);
        if(device == null) {
            _consoleOutput.ErrorLine($"Address '{ev.Address}' given by client does not match a device.");
            return;
        }

        await AppController.SetRoutingFor(device, ev.Route);
        // TODO JVC:
        // Update name
    }

    private async Task DeviceListUpdated(IWebSocketContext? context)
    {
        if(AppController == null)
            return;

        var contexts = context != null ? EnumerableExt.Wrap(context) : ActiveContexts;

        var devices = AppController.GetDeviceList();
        var response = JsonEgressMessage.CreateDeviceList(devices);
        foreach(var inner in contexts)
            await SendMessage(inner, response);

        foreach(var device in devices) {
            if(AppController.TryGetDeviceStatus(device, out var status)) {
                Debug.Assert(status != null);
                response = new JsonEgressMessage();
                response.AddDeviceStatus(device.Address, status);

                var routes = AppController.GetRoutingFor(device).Select(route =>
                    new JsonDeviceRoute(route.SourceEvent, route.TargetAddress, route.TargetFunctionality)
                ).ToList();
                response.AddDeviceRouting(device.Address, routes);

                var routingOptions = AppController.GetRoutingOptionsFor(device);
                if(routingOptions != null) {
                    var jsonConsumableEvents = routingOptions.ConsumedEvents.Select(ev => new JsonDeviceConsumableEvent(ev.Type, ev.Functionality)).ToList(); 
                    var jsonProvidedEvents = routingOptions.ProvidedEvents.Select(ev => new JsonDeviceProvidedEvent(ev.Type, ev.Name)).ToList();
                    response.AddDeviceRoutingOptions(device.Address, jsonProvidedEvents, jsonConsumableEvents);
                }

                foreach(var inner in contexts)
                    await SendMessage(inner, response);
            }
        }
    }

    public Task DeviceListUpdated() => DeviceListUpdated(null);

    public async Task DeviceStateUpdated(string address, IDeviceStatus deviceStatus)
    {
        if(AppController == null)
            return;
        
        var message = JsonEgressMessage.CreateDeviceStatus(address, deviceStatus);
        foreach(var context in ActiveContexts)
            await SendMessage(context, message);
    }
}
