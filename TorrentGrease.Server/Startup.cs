using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TorrentGrease.Data.Hosting;
using System;
using System.Linq;
using TorrentGrease.TorrentClient.Hosting;
using System.IO;
using ProtoBuf.Grpc.Server;
using TorrentGrease.Server.Services;
using Knowit.Grpc.Web;
using TorrentGrease.Server.Grpc;
using Microsoft.AspNetCore.DataProtection;
using System.Collections.Generic;
using TorrentGrease.TorrentStatisticsHarvester.Hosting;
using TorrentGrease.Hangfire.Hosting;
using Serilog;

namespace TorrentGrease.Server
{
    public class Startup
    {
        private readonly IConfiguration _config;

        public Startup(IConfiguration config)
        {
            _config = config;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCodeFirstGrpc();
            services.AddGrpcWeb();

            services.AddTorrentGreaseData(_config.GetConnectionString("DefaultConnection"));
            services.AddTorrentClient(_config.GetSection("torrentClient"));

            services.AddHangfire(_config);
            services.AddTorrentStatisticsHarvester();

            services.AddHealthChecks()
                .AddTorrentGreaseDataCheck();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }

            app.UseStaticFiles();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();
            app.UseGrpcWeb();

            app.UseSerilogRequestLogging();
            app.UseRouting();
            
            app.UseHangfire();
            //not yet
            //app.UseTorrentStatisticsHarvester(serviceProvider);

            app.UseHealthChecks("/health");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcServices();
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
