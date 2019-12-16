using System;
using System.Collections.Generic;
using System.Text;

namespace TorrentGrease.Shared.TorrentStatistics
{
    public class Torrent
    {
        public Torrent()
        {
        }

        public Torrent(TorrentClient.Torrent torrent)
        {
            InfoHash = torrent.InfoHash;
            WasInClientOnLastScan = true;
            Name = torrent.Name;
            Location = torrent.Location;
            LatestAddedDateTime = torrent.AddedDateTime;
            SizeInBytes = torrent.SizeInBytes;
            BytesOnDisk = torrent.BytesOnDisk;
            TotalUploadInBytes = 0;
        }

        public int Id { get; set; }
        public string InfoHash { get; set; }
        public bool WasInClientOnLastScan { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        /// <summary>
        /// Torrents can be removed and readded, to keep track of that we store the Added datetime
        /// </summary>
        public DateTime LatestAddedDateTime { get; set; }
        public long SizeInBytes { get; set; }
        public long BytesOnDisk { get; set; }
        public long TotalUploadInBytes { get; set; }
    }
}
