using EmbedIO;
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
            )
            .WithLocalSessionManager();

            server.StateChanged += (s, e) => _consoleOutput.InfoLine($"WebServer New State - {e.NewState}");

            return server;
        }
    }
}
