using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using System.Threading.Tasks;
using ProtoBuf.Grpc.Client;
using TorrentGrease.GrpcClient;
using Microsoft.Extensions.DependencyInjection;

namespace TorrentGrease.Client
{
    public class Program
    {
        public static async Task Main()
        {
            var builder = WebAssemblyHostBuilder.CreateDefault();
            builder.RootComponents.Add<App>("app");

            GrpcClientFactory.AllowUnencryptedHttp2 = true;
            builder.Services
                .AddBaseAddressHttpClient()
                .AddGrpcClients()
                .AddBlazorise(options =>
                {
                    options.ChangeTextOnKeyPress = true; // optional
                })
                .AddBootstrapProviders()
                .AddFontAwesomeIcons();

            //builder.Services
            //   .UseBootstrapProviders()
            //   .UseFontAwesomeIcons();
            await builder.Build().RunAsync();
        }
    }
}
