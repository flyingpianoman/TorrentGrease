using System;
using System.Collections.Generic;
using System.Text;

namespace TorrentGrease.Shared.TorrentStatistics
{
    public class Torrent
    {
        public int Id { get; set; }
        public string  Hash { get; set; }
        public bool WasInClientOnLastScan { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public int SizeInBytes { get; set; }
        public int BytesOnDisk { get; set; }
        public int TotalUploadInBits { get; set; }
    }
}
