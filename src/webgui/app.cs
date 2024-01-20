using EmbedIO;
using EmbedIO.WebApi;
using LightAssistant.Interfaces;

namespace LightAssistant.WebGUI
{
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
            _consoleOutput.MessageLine($"Web GUI listening on {_rootUrl}.");
            await _webServer.RunAsync();
        }

        private WebServer CreateWebServer()
        {
            var server = new WebServer(o => o
                    .WithUrlPrefix(_rootUrl)
                    .WithMode(HttpListenerMode.EmbedIO)
            );
         // First, we will configure our web server by adding Modules.
//                .WithLocalSessionManager()
//                .WithWebApi("/api", m => m
//                    .WithController<PeopleController>())
//                .WithModule(new WebSocketChatModule("/chat"))
//                .WithModule(new WebSocketTerminalModule("/terminal"))
//                .WithStaticFolder("/", HtmlRootPath, true, m => m
//                    .WithContentCaching(UseFileCache)) // Add static files after other modules to avoid conflicts
//                .WithModule(new ActionModule("/", HttpVerbs.Any, ctx => ctx.SendDataAsync(new { Message = "Error" })));

            // Listen for state changes.
            server.StateChanged += (s, e) => _consoleOutput.InfoLine($"WebServer New State - {e.NewState}");

            return server;
        }
    }
}
