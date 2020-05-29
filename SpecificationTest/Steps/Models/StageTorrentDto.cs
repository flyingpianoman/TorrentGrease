using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace SpecificationTest.Steps.Models
{
    class StageTorrentDto
    {
        public string Name { get; set; }
        public string Location { get; set; }
        public string TrackerAnnounceUrl1 { get; set; }
        public string TrackerAnnounceUrl2 { get; set; }
        public string TorrentFile1Path { get; set; }
        public long TorrentFile1SizeInKB { get; set; }
        public string TorrentFile2Path { get; set; }
        public long TorrentFile2SizeInKB { get; set; }
    }
}
