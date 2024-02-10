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

    private async Task SendMessage(IWebSocketContext context, JsonMessage msg)
    {
        await context.WebSocket.SendAsync(msg.Serialize(), true);
    }

    protected override async Task OnMessageReceivedAsync(IWebSocketContext context, byte[] buffer, IWebSocketReceiveResult result)
    {
        var inMsg = UTF8.GetString(buffer);
        var msg = $"Got the following '{inMsg}' from client.";
        await context.WebSocket.SendAsync(UTF8.GetBytes(msg), true);
    }

    private async Task DeviceListUpdated(IWebSocketContext? context)
    {
        if(AppController == null)
            return;

        var contexts = context != null ? EnumerableExt.Wrap(context) : ActiveContexts;

        var devices = AppController.GetDeviceList();
        var response = new JsonMessageDeviceList(devices);
        foreach(var inner in contexts)
            await SendMessage(inner, response);
    }

    public Task DeviceListUpdated() => DeviceListUpdated(null);
}
