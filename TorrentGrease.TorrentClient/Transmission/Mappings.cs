using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RpcEntity = Transmission.API.RPC.Entity;
namespace TorrentGrease.TorrentClient.Transmission
{
    public static class Mappings
    {
        internal static Shared.TorrentClient.Torrent ToSharedModel(this RpcEntity.TorrentInfo torrentInfo)
        {
            var sizeInGB = torrentInfo.TotalSize / 1024 / 1024 / 1024; //Convert bytes to GB
            return new Shared.TorrentClient.Torrent
            {
                Name = torrentInfo.Name,
                SizeInGB = sizeInGB
            };
        }
    }
}
