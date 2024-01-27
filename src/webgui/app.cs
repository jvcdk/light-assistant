using System.Text;
using EmbedIO;
using LightAssistant.Interfaces;
using RazorLight;

namespace LightAssistant.WebGUI
{
    internal class App : IDisposable, IUserInterface
    {
        private string TemplateRoot => Path.Combine(AppContext.BaseDirectory, "templates");
        private string StaticRoot => Path.Combine(AppContext.BaseDirectory, "static");
        private string TemplatePath(string name) => Path.Combine(TemplateRoot, name);
        private string SiteName => "Light Assistant";

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
            .WithStaticFolder("/css/", Path.Combine(StaticRoot, "css"), isImmutable: true, m => { m.ContentCaching = true; })
            .WithStaticFolder("/js/", Path.Combine(StaticRoot, "js"), isImmutable: true, m => { m.ContentCaching = true; })
            .WithStaticFolder("/images/", Path.Combine(StaticRoot, "images"), isImmutable: true, m => { m.ContentCaching = true; })
            .WithStaticFolder("/favicon.ico", Path.Combine(StaticRoot, "images/favicon.ico"), isImmutable: true, m => { m.ContentCaching = true; })
            .WithAction("/", HttpVerbs.Any, HandleRootUrl); // Must be last as it a catch-all

            server.HandleHttpException(async (ctx, ex) => {
                ctx.Response.StatusCode = ex.StatusCode;

                switch (ex.StatusCode) {
                    case 404:
                        var text = await _engine.CompileRenderAsync(TemplatePath("404.cshtml"), model: "");
                        await ctx.SendStringAsync(text, "text/html", Encoding.UTF8);
                        break;
                    default:
                        await ctx.SendStandardHtmlAsync(ex.StatusCode);
                        break;
                }
            });

            server.StateChanged += (s, e) => _consoleOutput.InfoLine($"WebServer New State - {e.NewState}");

            return server;
        }

        private async Task HandleRootUrl(IHttpContext context)
        {
            if(context.RequestedPath == "/") {
                var content = await PageRoot();
                await context.SendStringAsync(content, "text/html", Encoding.UTF8);
            }
            else
                throw new HttpException(404, "Content not available.");
        }

        public async Task<string> IncludeFile(string file, object model)
        {
            return await _engine.CompileRenderAsync(file, model);
        }

        public async Task<string> PageRoot()
        {
            var model = new { 
                SiteName,
            };
            return await _engine.CompileRenderAsync("root.cshtml", model);
        }
    }
}
