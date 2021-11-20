using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProtoBuf.Grpc.Client;
using ProtoBuf.Grpc.Configuration;
using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using TorrentGrease.Shared.ServiceContracts;

namespace TorrentGrease.Client.ServiceClientExtensions
{
    public static class ServiceCollectionGrpcExtensions
    {
        public static IServiceCollection AddGrpcClients(this IServiceCollection services)
        {
            RegisterGrpcChannel(services);

            var grpcServiceInterfaceTypes = ReflectionHelper.GetGrpcServiceInterfaceTypes();

            foreach (var grpcServiceInterfaceType in grpcServiceInterfaceTypes)
            {
                services.AddGrpcClient(grpcServiceInterfaceType);
            }

            return services;
        }

        private static void RegisterGrpcChannel(IServiceCollection services)
        {
            services.AddSingleton(serviceProvider =>
            {
                var navigationManager = serviceProvider.GetRequiredService<NavigationManager>();
                var backendUrl = navigationManager.BaseUri;

                // GrpcWebText can be used because server streaming requires it. 
                // If server streaming is not used in your app
                // then GrpcWeb is recommended because it produces smaller messages.
                var gRpcWebHttpHandler = new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler())
                {
                    HttpVersion = new Version(1, 1)
                };

                return GrpcChannel.ForAddress(backendUrl, new GrpcChannelOptions
                {
                    HttpHandler = gRpcWebHttpHandler
                });
            });
        }

        public static IServiceCollection AddGrpcClient(this IServiceCollection services, Type grpcServiceType)
        {
            var method = typeof(ServiceCollectionGrpcExtensions)
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                .Single(m => m.Name == nameof(AddGrpcClient) && m.ContainsGenericParameters)
                .MakeGenericMethod(grpcServiceType);

            method.Invoke(null, new[] { services });

            return services;
        }

        public static IServiceCollection AddGrpcClient<TGrpcClient>(this IServiceCollection services)
            where TGrpcClient : class
        {
            services.AddScoped(serviceProvider =>
            {
                var grpcChannel = serviceProvider.GetRequiredService<GrpcChannel>();
                return grpcChannel.CreateGrpcService<TGrpcClient>();
            });

            return services;
        }
    }
}
