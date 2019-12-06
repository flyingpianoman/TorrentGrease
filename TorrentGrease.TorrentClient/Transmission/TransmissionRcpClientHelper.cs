using System;
using System.Collections.Generic;
using System.Text;
using RpcClient = Transmission.API.RPC;

namespace TorrentGrease.TorrentClient.Hosting
{
    public static class TransmissionRcpClientHelper
    {
        public static RpcClient.Client CreateTransmissionRpcClient(TorrentClientSettings settings)
        {
            var rpcUrl = settings.Url.AbsoluteUri.TrimEnd('/') + "/transmission/rpc";
            return new RpcClient.Client(rpcUrl, login: settings.Username, password: settings.Password);
        }
    }
}
