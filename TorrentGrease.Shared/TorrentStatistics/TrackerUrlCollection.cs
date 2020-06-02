using System;
using System.Collections.Generic;
using System.Text;

namespace TorrentGrease.Shared.TorrentStatistics
{
    public class TrackerUrlCollection
    {
        public int Id { get; set; }
        public byte[] CollectionHash { get; set; }
        public List<TrackerUrl> TrackerUrls { get; set; }
    }
}
