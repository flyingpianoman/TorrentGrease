using System;
using System.Collections.Generic;
using System.Text;

namespace TorrentGrease.Shared.TorrentStatistics
{
    public class TrackerUrl
    {
        public TrackerUrl()
        {
        }

        public TrackerUrl(string url)
        {
            this.Url = url;
        }

        public int Id { get; set; }
        public int TrackerUrlCollectionId { get; set; }
        public string Url { get; set; }
    }
}
