using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtoBuf.Grpc.Client;
using System;
using System.Threading.Tasks;
using TorrentGrease.Shared.ServiceContracts;

namespace IntegrationTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async Task TestGrpcEndpoint()
        {
            GrpcClientFactory.AllowUnencryptedHttp2 = true;
            using var channel = GrpcChannel.ForAddress("http://localhost:5657");
            var policyService = channel.CreateGrpcService<IPolicyService>();
            await policyService.GetAllPoliciesAsync();
        }
    }
}
