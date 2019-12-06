using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;
using ProtoBuf.Grpc.Client;
using TorrentGrease.GrpcClient;

namespace TorrentGrease.Client
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            GrpcClientFactory.AllowUnencryptedHttp2 = true;
            services.AddGrpcClients();

            services
                .AddBlazorise(options =>
                {
                    options.ChangeTextOnKeyPress = true; // optional
                })
                .AddBootstrapProviders()
                .AddFontAwesomeIcons();
        }

        public void Configure(IComponentsApplicationBuilder app)
        {
            app.Services
               .UseBootstrapProviders()
               .UseFontAwesomeIcons();

            app.AddComponent<App>("app");
        }
    }
}
