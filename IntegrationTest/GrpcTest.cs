using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtoBuf.Grpc.Client;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using TorrentGrease.Shared.ServiceContracts;

namespace IntegrationTest
{
    [TestClass]
    public class GrpcTest
    {
        [TestMethod]
        public async Task TestGrpcEndpoint()
        {
            GrpcClientFactory.AllowUnencryptedHttp2 = true;
            using var channel = GrpcChannel.ForAddress("http://localhost:5657", new GrpcChannelOptions { Credentials = ChannelCredentials.Insecure });
            var policyService = channel.CreateGrpcService<IPolicyService>();
            await policyService.GetAllPoliciesAsync();
        }

        [TestMethod]
        public async Task TestGrpcWebEndpointHttp2()
        {
            // GrpcWebText can be used because server streaming requires it. 
            // If server streaming is not used in your app
            // then GrpcWeb is recommended because it produces smaller messages.
            var gRpcWebHttpHandler = new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler());

            var grpcChannel = GrpcChannel.ForAddress("http://localhost:5657", new GrpcChannelOptions
            {
                HttpHandler = gRpcWebHttpHandler
            });
            var policyService = grpcChannel.CreateGrpcService<IPolicyService>();

            //Act
            await policyService.GetAllPoliciesAsync();
        }

        [TestMethod]
        public async Task TestGrpcWebEndpointHttp1()
        {
            // GrpcWebText can be used because server streaming requires it. 
            // If server streaming is not used in your app
            // then GrpcWeb is recommended because it produces smaller messages.
            var gRpcWebHttpHandler = new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler())
            {
                HttpVersion = new Version(1, 1)
            };

            var grpcChannel = GrpcChannel.ForAddress("http://localhost:5656", new GrpcChannelOptions
            {
                HttpHandler = gRpcWebHttpHandler
            });
            var policyService = grpcChannel.CreateGrpcService<IPolicyService>();

            //Act
            await policyService.GetAllPoliciesAsync();
        }
    }
}
