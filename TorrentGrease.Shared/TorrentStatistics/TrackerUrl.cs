using System;
using System.Collections.Generic;
using System.Text;

namespace TorrentGrease.Shared.TorrentStatistics
{
    public class TrackerUrl
    {
        public int Id { get; set; }
        public int TrackerUrlCollectionId { get; set; }
        public string Url { get; set; }
    }
}
