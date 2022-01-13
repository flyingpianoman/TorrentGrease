using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using Serilog.Filters;
using System.Threading.Tasks;
using TorrentGrease.Data.Hosting;
using TorrentGrease.Hangfire.Hosting;
using TorrentGrease.TorrentStatisticsHarvester;
using TorrentGrease.TorrentClient.Hosting;
using ProtoBuf.Grpc.Server;
using TorrentGrease.TorrentStatisticsHarvester.Hosting;
using TorrentGrease.Server.CrossCutting;


namespace TorrentGrease.Server
{
    public class Program
    {
        //private const string LogOutputTemplate = "{Timestamp:MM-dd HH:mm:ss} [{Level:u3}] [{SourceContext:l}] {Message}{NewLine}{Exception}";
        private const string LogOutputTemplate = "{Timestamp:MM-dd HH:mm:ss} [{Level:u3}] {Message}{NewLine}{Exception}";

        public static async Task Main(string[] args)
        {
            var builder = CreateAppBuilder(args);
            ConfigureServices(builder);

            var app = builder.Build();
            ConfigureApp(app);

            await MigrageDBAsync(app);

            await app.RunAsync();
        }

        private static void ConfigureApp(WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }

            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();
            app.UseSerilogRequestLogging();
            app.UseRouting();

            app.UseHangfire();
            app.UseGrpcWeb(new GrpcWebOptions() { DefaultEnabled = true });
            //not yet
            //app.UseTorrentStatisticsHarvester(serviceProvider);

            app.UseHealthChecks("/health");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcServices();
                endpoints.MapFallbackToFile("index.html");
            });
        }

        private static WebApplicationBuilder CreateAppBuilder(string[] args)
        {
            var builder = WebApplication
                            .CreateBuilder(args);

            builder.Host
                .UseSerilog((hostingContext, loggerConfiguration) => ConfigureSerilog(hostingContext, loggerConfiguration));
                
            builder.WebHost
                .UseConfiguration(new ConfigurationBuilder()
                    .AddCommandLine(args)
                    .AddEnvironmentVariables()
                    .Build())
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

            return builder;
        }

        private static void ConfigureServices(WebApplicationBuilder builder)
        {
            var configuration = builder.Configuration;
            var services = builder.Services;

            services.AddCodeFirstGrpc(cfg => { cfg.ResponseCompressionLevel = System.IO.Compression.CompressionLevel.Optimal; });

            services.AddTorrentGreaseData(configuration.GetConnectionString("DefaultConnection"));
            services.AddTorrentClient(configuration.GetSection("torrentClient"));

            services.AddHangfire(configuration);
            services.AddTorrentStatisticsHarvester();

            services.AddHealthChecks()
                .AddTorrentGreaseDataCheck();
        }

        private static async Task MigrageDBAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var dbInitializer = services.GetRequiredService<TorrentGreaseDbInitializer>();
            await dbInitializer.InitializeAsync();
        }

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
