using ProtoBuf.Grpc.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace TorrentGrease.GrpcClient
{
    public static class GrpcWebClientFactory
    {
        public static TGrpcClient CreateClient<TGrpcClient>(HttpClient httpClient)
            where TGrpcClient : class
        {
            var grpcWebCallInvoker = new GrpcWebCallInvoker(httpClient);
            return ClientFactory.Default.CreateClient<TGrpcClient>(grpcWebCallInvoker);
        }
    }
}
