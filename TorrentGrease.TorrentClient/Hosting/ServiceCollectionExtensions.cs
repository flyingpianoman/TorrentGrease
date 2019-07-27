using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using RpcClient = Transmission.API.RPC;
using System.Collections.Generic;
using System.Text;
using TorrentGrease.TorrentClient.Transmission;

namespace TorrentGrease.TorrentClient.Hosting
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTorrentClient(this IServiceCollection services, IConfigurationSection torrentClientConfig)
        {
            ConfigureTorrentClient(services, torrentClientConfig ?? throw new ArgumentNullException(nameof(torrentClientConfig)));
            return services;
        }

        private static void ConfigureTorrentClient(IServiceCollection services, IConfigurationSection torrentClientConfig)
        {
            services.Configure<TorrentClientSettings>(torrentClientConfig);
            var client = torrentClientConfig.GetValue<string>("client");

            switch (client?.ToLowerInvariant())
            {
                case "transmission":
                    services.AddTransient(BuildTransmissionRpcClient);
                    services.AddTransient<ITorrentClient, TransmissionClient>();
                    break;

                default:
                    throw new NotImplementedException($"Only transmission is supported atm, unknown client: '{client}'");
            }
        }

        private static RpcClient.Client BuildTransmissionRpcClient(IServiceProvider sp)
        {
            var settings = sp.GetRequiredService<IOptions<TorrentClientSettings>>().Value;
            return TransmissionRcpClientHelper.CreateTransmissionRpcClient(settings);
        }
    }
}
