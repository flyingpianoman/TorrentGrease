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
                Location = torrentInfo.DownloadDir,
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
