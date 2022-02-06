using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using ProtoBuf.Grpc.Client;
using TestUtils;
using TorrentGrease.Shared.ServiceContracts;

namespace SpecificationTest.Crosscutting
{
    internal static class IGprcClientExtensions
    {
        internal static void RegisterGrpcClients(this DIContainer diContainer)
        {
            GrpcClientFactory.AllowUnencryptedHttp2 = true;
            var channel = GrpcChannel.ForAddress("http://localhost:5657", new GrpcChannelOptions { Credentials = ChannelCredentials.Insecure });
            diContainer.Register(channel);

            diContainer.Register(channel.CreateGrpcService<ITorrentService>());
        }
    }
}
