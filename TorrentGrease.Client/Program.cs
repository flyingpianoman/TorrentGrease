using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using System.Threading.Tasks;
using ProtoBuf.Grpc.Client;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using TorrentGrease.Client.ServiceClientExtensions;
using Microsoft.AspNetCore.Components.Web;
using TorrentGrease.Client;

var builder = WebAssemblyHostBuilder.CreateDefault();

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

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var host = builder.Build();
await host.RunAsync();