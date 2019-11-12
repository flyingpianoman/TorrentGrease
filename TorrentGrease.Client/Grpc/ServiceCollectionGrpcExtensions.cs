using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using TorrentGrease.Shared.ServiceContracts;

namespace TorrentGrease.Client.Grpc
{
    public static class ServiceCollectionGrpcExtensions
    {
        public const string BackendUrl = "http://localhost:5656";

        public static IServiceCollection AddGrpcClients(this IServiceCollection services)
        {
            var grpcServiceInterfaceTypes = ReflectionHelper.GetGrpcServiceInterfaceTypes();

            foreach (var grpcServiceInterfaceType in grpcServiceInterfaceTypes)
            {
                services.AddGrpcClient(grpcServiceInterfaceType);
            }

            return services;
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
                var httpClient = new HttpClient { BaseAddress = new Uri(BackendUrl) };
                var grpcWebCallInvoker = new GrpcWebCallInvoker(httpClient);
                return ProtoBuf.Grpc.Configuration.ClientFactory.Default.CreateClient<TGrpcClient>(grpcWebCallInvoker);
            });

            return services;
        }
    }
}
