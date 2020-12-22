using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using System.Threading.Tasks;
using ProtoBuf.Grpc.Client;
using TorrentGrease.GrpcClient;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

namespace TorrentGrease.Client
{
    public class Program
    {
        public static async Task Main()
        {
            var builder = WebAssemblyHostBuilder.CreateDefault();
            builder.RootComponents.Add<App>("#app");

            GrpcClientFactory.AllowUnencryptedHttp2 = true;
            builder.Services
                .AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) })
                .AddGrpcClients()
                .AddBlazorise(options =>
                {
                    options.ChangeTextOnKeyPress = true; // optional
                })
                .AddBootstrapProviders()
                .AddFontAwesomeIcons();

            var host = builder.Build();

            host.Services
               .UseBootstrapProviders()
               .UseFontAwesomeIcons();

            await host.RunAsync();
        }
    }
}
