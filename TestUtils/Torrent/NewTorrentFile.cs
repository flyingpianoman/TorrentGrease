using System;
using System.Collections.Generic;
using System.Text;

namespace TestUtils.Torrent
{
    public class NewTorrentFile
    {
        public string Name { get; set; }
        public IEnumerable<CreateTorrentFileMapping> FileMappings { get; set; }
        public IList<string> TrackerAnnounceUrls { get; set; } = new List<string>();
        public bool IsPrivate { get; set; } = true;
    }
}
