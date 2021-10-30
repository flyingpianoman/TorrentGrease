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
                ID = torrentInfo.ID,
                Name = torrentInfo.Name,
                SizeInBytes = torrentInfo.TotalSize,
                BytesOnDisk = torrentInfo.TotalSize - torrentInfo.LeftUntilDone,
                InfoHash = torrentInfo.HashString,
                Location = GetRealLocation(torrentInfo),
                TotalUploadInBytes = torrentInfo.UploadedEver,
                TrackerUrls = torrentInfo.Trackers
                    .Select(t => new Uri(t.announce).Authority)
                    .ToList(),
                AddedDateTime = DateTimeOffset.FromUnixTimeSeconds(torrentInfo.AddedDate).UtcDateTime,
                Files = torrentInfo.Files.ToSharedModel(),
                Error = torrentInfo.ErrorString,
                TorrentFilePath = torrentInfo.TorrentFile
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


        internal static Shared.TorrentClient.TorrentFile[] ToSharedModel(this RpcEntity.TransmissionTorrentFiles[] torrentFiles)
        {
            return torrentFiles.Select(tf => tf.ToSharedModel()).ToArray();
        }

        internal static Shared.TorrentClient.TorrentFile ToSharedModel(this RpcEntity.TransmissionTorrentFiles torrentFile)
        {
            return new Shared.TorrentClient.TorrentFile
            {
                FileLocationInTorrent = torrentFile.Name,
                SizeInBytes = Convert.ToInt64(torrentFile.Length)
            };
        }
    }
}
