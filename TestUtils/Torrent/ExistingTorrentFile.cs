using System;
using System.Collections.Generic;
using System.Text;

namespace TestUtils.Torrent
{
    public class ExistingTorrentFile
    {
        public string Name { get; set; }
        public IEnumerable<InnerTorrentFileInfo> InnerTorrentFiles { get; set; }
        public string TrackerAnnounceUrl { get; set; }
        public bool IsPrivate { get; set; } = true;
    }
}
