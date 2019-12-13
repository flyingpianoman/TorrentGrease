using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TorrentGrease.Data.Hosting;
using TorrentGrease.Hangfire.Hosting;

namespace TorrentGrease.Server
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
#if DEBUG
            CorrectBlazorConfigPaths();
#endif
            var host = BuildWebHost(args);
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var dbInitializer = services.GetRequiredService<TorrentGreaseDbInitializer>();
                await dbInitializer.InitializeAsync();
            }
            await host.RunAsync();
        }

        private static void CorrectBlazorConfigPaths()
        {
            const string blazorConfigPath = @"/app/bin/Debug/netcoreapp3.1/TorrentGrease.Client.blazor.config";
            var blazorConfig = File.ReadAllText(blazorConfigPath);
            blazorConfig = Regex.Replace(blazorConfig, @"[a-zA-Z]:[\/\\].+?[\/\\]TorrentGrease.Client[\/\\]", "/TorrentGrease.Client/")
                .Replace('\\', '/');
            File.WriteAllText(blazorConfigPath, blazorConfig);
        }

        public static IHost BuildWebHost(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webHostBuilder =>
                {
                    webHostBuilder
                        .UseConfiguration(new ConfigurationBuilder()
                            .AddCommandLine(args)
                            .AddEnvironmentVariables()
                            .Build())
                        .UseStartup<Startup>()
                        .ConfigureKestrel(options =>
                        {
                            options.ListenAnyIP(port: 5656, listenOptions => //For Blazor, health endpoint & gRPC-web
                            {
                                listenOptions.Protocols = HttpProtocols.Http1;
                            });
                            options.ListenAnyIP(port: 5657, listenOptions => //For gRPC
                            {
                                listenOptions.Protocols = HttpProtocols.Http2;
                            });
                        });
                })
                .Build();
    }
}
