using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Threading.Tasks;
using TorrentGrease.Data.Hosting;

namespace TorrentGrease.Server
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            //Temp fix for https://github.com/aspnet/Blazor/issues/376
#if DEBUG
            MoveWwwrootToDistDir();
#endif
            var host = BuildWebHost(args);
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var dbInitializer = services.GetRequiredService<DbInitializer>();
                await dbInitializer.InitializeAsync();
            }
            await host.RunAsync();
        }

        private static void MoveWwwrootToDistDir()
        {
            var wwwRootDir = new DirectoryInfo("/TorrentGrease.Client/dist_wwwroot");
            var distDir = new DirectoryInfo("/TorrentGrease.Client/bin/Debug/netstandard2.1/dist");
            CopyAll(wwwRootDir, distDir);
        }

        public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (var fi in source.GetFiles())
            {
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (var diSourceSubDir in source.GetDirectories())
            {
                var nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
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
