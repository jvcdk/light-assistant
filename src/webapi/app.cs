using EmbedIO;
using EmbedIO.WebSockets;
using LightAssistant.Interfaces;
using static System.Text.Encoding;

namespace LightAssistant.WebApi;

internal class App : IDisposable, IUserInterface
{
    private readonly IConsoleOutput _consoleOutput;
    private readonly WebServer _webServer;
    private readonly string _rootUrl;

    public App(IConsoleOutput consoleOutput, string hostAddr, int port)
    {
        _consoleOutput = consoleOutput;
        _rootUrl = $"http://{hostAddr}:{port}";

        _webServer = CreateWebServer();
    }

    public void Dispose()
    {
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
        .WithModule(new MyWebSocketModule("/", _consoleOutput));

        server.StateChanged += (s, e) => _consoleOutput.InfoLine($"WebServer New State - {e.NewState}");

        return server;
    }
}


public class MyWebSocketModule : WebSocketModule
{
    private readonly IConsoleOutput _consoleOutput;

    public MyWebSocketModule(string urlPath, IConsoleOutput consoleOutput)
        : base(urlPath, true)
    {
        _consoleOutput = consoleOutput;
    }

    protected override async Task OnMessageReceivedAsync(IWebSocketContext context, byte[] buffer, IWebSocketReceiveResult result)
    {
        _consoleOutput.ErrorLine(result. EndOfMessage.ToString());
        var inMsg = UTF8.GetString(buffer);
        var msg = $"Got the following '{inMsg}' from client.";
        await context.WebSocket.SendAsync(UTF8.GetBytes(msg), true);
    }
}
