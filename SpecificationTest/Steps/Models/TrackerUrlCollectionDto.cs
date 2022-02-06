using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecificationTest.Steps.Models
{
    internal class TrackerUrlCollectionDto
    {
        public string TrackerAnnounceUrl1 { get; set; }
        public string TrackerAnnounceUrl2 { get; set; }
        public string TrackerAnnounceUrl3 { get; set; }
        public IEnumerable<string> TrackerAnnounceUrls => new string[] { TrackerAnnounceUrl1, TrackerAnnounceUrl2, TrackerAnnounceUrl3 }.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

    }
}
