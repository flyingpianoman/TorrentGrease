using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using Serilog.Filters;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using TorrentGrease.Data.Hosting;
using TorrentGrease.Hangfire.Hosting;
using TorrentGrease.TorrentStatisticsHarvester;

namespace TorrentGrease.Server
{
    public class Program
    {
        //private const string LogOutputTemplate = "{Timestamp:MM-dd HH:mm:ss} [{Level:u3}] [{SourceContext:l}] {Message}{NewLine}{Exception}";
        private const string LogOutputTemplate = "{Timestamp:MM-dd HH:mm:ss} [{Level:u3}] {Message}{NewLine}{Exception}";

        public static async Task Main(string[] args)
        {

#if DEBUG
            CorrectPathsInStaticWebAssets();
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

        private static void CorrectPathsInStaticWebAssets()
        {
            const string serverStaticWebAssetsPath = @"/app/bin/Debug/net5.0/TorrentGrease.Server.StaticWebAssets.xml";
            var serverXmlDoc = new XmlDocument();
            serverXmlDoc.Load(serverStaticWebAssetsPath);
            var rootNode = serverXmlDoc.SelectSingleNode("StaticWebAssets") ?? throw new InvalidDataException();
            

            const string clientStaticWebAssetsPath = @"/app/bin/Debug/net5.0/TorrentGrease.Client.StaticWebAssets.xml";
            var clientXmlDoc = new XmlDocument();
            clientXmlDoc.Load(clientStaticWebAssetsPath);
            var contentRootNodes = clientXmlDoc.SelectNodes("StaticWebAssets/ContentRoot") ?? throw new InvalidDataException();

            foreach (var contentRootNode in contentRootNodes.Cast<XmlNode>())
            {
                var basePathAttr = contentRootNode?.Attributes?["BasePath"] ?? throw new NotSupportedException();
                var pathAttr = contentRootNode.Attributes["Path"];
                var modifiedPath = pathAttr?.Value.Replace('\\', '/') ?? throw new NotSupportedException();

                if (basePathAttr.Value == "/")
                {
                    modifiedPath = Regex.Replace(modifiedPath, @"[a-zA-Z]:[\/].+?[\/]TorrentGrease.Client[\/]", "/src/TorrentGrease.Client/");
                }
                else if (basePathAttr.Value.StartsWith("_content/"))
                {
                    modifiedPath = Regex.Replace(modifiedPath, @"[a-zA-Z]:\/.+?\/\.nuget\/", "/root/.nuget/");

                    //Also add these to the server xml
                    var newNode = serverXmlDoc.CreateElement("ContentRoot");
                    newNode.SetAttribute("BasePath", basePathAttr.Value);
                    newNode.SetAttribute("Path", pathAttr.Value);
                    rootNode.AppendChild(newNode);
                }
                else
                {
                    throw new NotImplementedException();
                }

                pathAttr.Value = modifiedPath;
            }

            clientXmlDoc.Save(clientStaticWebAssetsPath);
            serverXmlDoc.Save(serverStaticWebAssetsPath);
        }
        public static IHost BuildWebHost(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog((hostingContext, loggerConfiguration) => ConfigureSerilog(hostingContext, loggerConfiguration))
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

        private static LoggerConfiguration ConfigureSerilog(HostBuilderContext hostingContext, LoggerConfiguration loggerConfiguration)
        {
            var loggerConfig = ConfigureLogger(hostingContext, loggerConfiguration);
            loggerConfig.WriteTo.Console(outputTemplate: LogOutputTemplate);

            return loggerConfig
                .WriteTo.Logger(lc =>
                {
                    ConfigureLogger(hostingContext, lc);
                    lc.Filter.ByExcluding(Matching.FromSource<TorrentStatisticsHarvesterJob>());
                    WriteToFile(lc, "logs/TorrentGrease.{{Date}}.txt");
                })
                .WriteTo.Logger(lc =>
                {
                    ConfigureLogger(hostingContext, lc);
                    WriteToOwnFile<TorrentStatisticsHarvesterJob>(lc);
                });
        }

        private static LoggerConfiguration ConfigureLogger(HostBuilderContext hostingContext, LoggerConfiguration loggerConfiguration)
        {
            return loggerConfiguration
                .ReadFrom.Configuration(hostingContext.Configuration)
                .Enrich.FromLogContext();
        }

        private static void WriteToOwnFile<TCategory>(LoggerConfiguration lc)
        {
            lc.Filter.ByIncludingOnly(Matching.FromSource<TCategory>());
            WriteToFile(lc, $"logs/TorrentGrease.{typeof(TCategory).Name}.{{Date}}.txt");
        }

        private static void WriteToFile(LoggerConfiguration lc, string fileLoc)
        {
            lc.WriteTo.File(fileLoc,
                            rollingInterval: RollingInterval.Day,
                            fileSizeLimitBytes: 100 * 1024 * 1024, //100mb
                            rollOnFileSizeLimit: true,
                            levelSwitch: new LoggingLevelSwitch(Serilog.Events.LogEventLevel.Verbose),
                            restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Verbose,
                            outputTemplate: LogOutputTemplate);
        }
    }
}
