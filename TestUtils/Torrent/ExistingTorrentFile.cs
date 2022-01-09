using System;
using System.Collections.Generic;
using System.Text;

namespace TestUtils.Torrent
{
    public class ExistingTorrentFile
    {
        public string Name { get; init; }
        public IEnumerable<InnerTorrentFileInfo> InnerTorrentFiles { get; init; }
        public string TrackerAnnounceUrl { get; init; }
        public bool IsPrivate { get; init; } = true;
    }
}
