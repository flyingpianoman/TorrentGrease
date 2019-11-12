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

            services.AddMvc().AddNewtonsoftJson();
            services.AddHealthChecks()
                .AddTorrentGreaseDataCheck();

            services.AddResponseCompression(opts =>
            {
                opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "application/octet-stream" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseResponseCompression();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBlazorDebugging();
            }

            UseClientSideBlazorFiles(app);
            app.UseGrpcWeb();
            app.UseRouting();

            app.UseHealthChecks("/health");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcServices();

                endpoints.MapDefaultControllerRoute();
                endpoints.MapFallbackToClientSideBlazor<Client.Startup>("index.html");
            });
        }

        private void UseClientSideBlazorFiles(IApplicationBuilder app)
        {
            var clientAssemblyPathOverride = _config.GetValue<string>("CLIENT_APP_ASSEMBLY_PATH_OVERRIDE");
            if (!string.IsNullOrEmpty(clientAssemblyPathOverride))
            {
                app.UseClientSideBlazorFiles(clientAssemblyPathOverride);
            }
            else
            {
                app.UseClientSideBlazorFiles<Client.Startup>();
            }
        }
    }
}
