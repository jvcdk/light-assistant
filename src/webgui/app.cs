using System.Text;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using LightAssistant.Interfaces;
using RazorLight;

namespace LightAssistant.WebGUI
{
    internal class App : IDisposable, IUserInterface
    {
        private string TemplateRoot => Path.Combine(AppContext.BaseDirectory, "templates");
        private string TemplatePath(string name) => Path.Combine(TemplateRoot, name);

        private readonly IConsoleOutput _consoleOutput;
        private readonly WebServer _webServer;
        private readonly RazorLightEngine _engine;
        private readonly string _rootUrl;

        public App(IConsoleOutput consoleOutput, string hostAddr, int port)
        {
            _consoleOutput = consoleOutput;
            _rootUrl = $"http://{hostAddr}:{port}";

            _webServer = CreateWebServer();
            _engine = new RazorLightEngineBuilder()
                .UseFileSystemProject(TemplateRoot)
                .UseMemoryCachingProvider()
                .Build();
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
            .WithLocalSessionManager()
            .WithWebApi("/", m => m.WithController(() => new RoutingProxy(this)));;

            server.HandleHttpException(async (ctx, ex) => {
                ctx.Response.StatusCode = ex.StatusCode;

                switch (ex.StatusCode) {
                    case 404:
                        var text = await _engine.CompileRenderAsync(TemplatePath("404.cshtml"), model: "");
                        await ctx.SendStringAsync(text, "text/html", Encoding.UTF8);
                        break;
                    default:
                        // Handle other HTTP Status codes or call the default handler 'SendStandardHtmlAsync'
                        await ctx.SendStandardHtmlAsync(ex.StatusCode);
                        break;
                }
            });

            // Listen for state changes.
            server.StateChanged += (s, e) => _consoleOutput.InfoLine($"WebServer New State - {e.NewState}");

            return server;
        }

        public async Task<string> PageRoot()
        {
            var model = new { Name = "John Doe" };
            return await _engine.CompileRenderAsync("root.cshtml", model);
        }

        
        // Proxy class to prevent App to be disposed.
        private class RoutingProxy : WebApiController
        {
            private readonly App _app;

            internal RoutingProxy(App app)
            {
                _app = app;
            }

            [Route(HttpVerbs.Get, "/")]
            public async Task<string> PageRoot() => await _app.PageRoot();
        }
    }
}
