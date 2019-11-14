using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;
using ProtoBuf.Grpc.Client;
using System;
using System.Net.Http;
using TorrentGrease.GrpcClient;
using TorrentGrease.Shared.ServiceContracts;

namespace TorrentGrease.Client
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            GrpcClientFactory.AllowUnencryptedHttp2 = true;
            services.AddGrpcClients();
        }

        public void Configure(IComponentsApplicationBuilder app)
        {
            app.AddComponent<App>("app");
        }
    }
}
