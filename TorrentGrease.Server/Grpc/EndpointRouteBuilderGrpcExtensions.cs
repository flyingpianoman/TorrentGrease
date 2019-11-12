using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;
using TorrentGrease.Server.Services;

namespace TorrentGrease.Server.Grpc
{
    public static class EndpointRouteBuilderGrpcExtensions
    {
        public static IEndpointRouteBuilder MapGrpcServices(this IEndpointRouteBuilder services)
        {
            var grpcServiceInterfaceTypes = typeof(EndpointRouteBuilderGrpcExtensions).Assembly
                .GetTypes()
                .Where(t => t.Namespace == typeof(PolicyService).Namespace) //could be made more specific by checking if the service implements a interface that contains the 'ServiceContract' interface
                .ToArray();

            foreach (var grpcServiceInterfaceType in grpcServiceInterfaceTypes)
            {
                services.MapGrpcService(grpcServiceInterfaceType);
            }

            return services;
        }

        public static IEndpointRouteBuilder MapGrpcService(this IEndpointRouteBuilder services, Type grpcServiceType)
        {
            var method = typeof(GrpcEndpointRouteBuilderExtensions)
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                .Single(m => m.Name == nameof(GrpcEndpointRouteBuilderExtensions.MapGrpcService))
                .MakeGenericMethod(grpcServiceType);

            method.Invoke(null, new[] { services });

            return services;
        }
    }
}
