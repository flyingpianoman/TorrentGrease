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
            return new Shared.TorrentClient.Torrent
            {
                Name = torrentInfo.Name,
                SizeInBytes = torrentInfo.TotalSize,
                BytesOnDisk = torrentInfo.TotalSize - torrentInfo.LeftUntilDone,
                InfoHash = torrentInfo.HashString,
                Location = GetRealLocation(torrentInfo),
                TotalUploadInBytes = torrentInfo.UploadedEver,
                TrackerUrls = torrentInfo.Trackers
                    .Select(t => new Uri(t.announce).Authority)
                    .ToList(),
                AddedDateTime = DateTimeOffset.FromUnixTimeSeconds(torrentInfo.AddedDate).UtcDateTime
            };
        }

        private static string GetRealLocation(RpcEntity.TorrentInfo torrentInfo)
        {
            var pathSeperator = torrentInfo.DownloadDir.Contains('/')
                ? "/"
                : "\\";

            return torrentInfo.Files.Length > 1
                ? torrentInfo.DownloadDir + pathSeperator + torrentInfo.Name
                : torrentInfo.DownloadDir;
        }
    }
}
