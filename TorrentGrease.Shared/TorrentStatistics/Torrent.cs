using System;
using System.Collections.Generic;
using System.Text;

namespace TorrentGrease.Shared.TorrentStatistics
{
    public class Torrent
    {
        public int Id { get; set; }
        public string InfoHash { get; set; }
        public bool WasInClientOnLastScan { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public long SizeInBytes { get; set; }
        public long BytesOnDisk { get; set; }
        public long TotalUploadInBytes { get; set; }
    }
}
