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
        public int UploadDeltaSinceLastSnapshotInBits { get; set; }
        public int TotalUploadInBits { get; set; }
        public int TrackerUrlCollectionId { get; set; }
        public TrackerUrlCollection TrackerUrlCollection { get; set; }
    }
}
