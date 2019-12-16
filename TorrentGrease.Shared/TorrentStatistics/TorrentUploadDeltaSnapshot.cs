using System;
using System.Collections.Generic;
using System.Text;

namespace TorrentGrease.Shared.TorrentStatistics
{
    public class TorrentUploadDeltaSnapshot
    {
        public int Id { get; set; }
        public DateTime DateTime { get; set; }
        public int TorrentId { get; set; }
        public Torrent Torrent { get; set; }
        /// <summary>
        /// Since we keep track of torrents even after they're re-added, we need the upload of the torrent in the client so we can calculate diffs
        /// </summary>
        public long TotalUploadForThisTorrentInBytes { get; set; }
        public long UploadDeltaSinceLastSnapshotInBytes { get; set; }
        public long TotalUploadInBytes { get; set; }
        public int TrackerUrlCollectionId { get; set; }
        public TrackerUrlCollection TrackerUrlCollection { get; set; }
    }
}
