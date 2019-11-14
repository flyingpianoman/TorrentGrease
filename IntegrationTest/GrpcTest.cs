using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtoBuf.Grpc.Client;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using TorrentGrease.GrpcClient;
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
        public async Task TestGrpcWebEndpoint()
        {
            GrpcClientFactory.AllowUnencryptedHttp2 = true;
            using var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5656") };
            var policyService = GrpcWebClientFactory.CreateClient<IPolicyService>(httpClient);
            await policyService.GetAllPoliciesAsync();
        }
    }
}
