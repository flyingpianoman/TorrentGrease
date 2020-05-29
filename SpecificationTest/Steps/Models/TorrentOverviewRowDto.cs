using System;
using System.Collections.Generic;
using System.Text;

namespace SpecificationTest.Steps.Models
{
    public class TorrentOverviewRowDto
    {
        public string Name { get; set; }
        public Decimal GBsOnDisk { get; set; }
        public Decimal TotalSizeInGB { get; set; }
        public Decimal TotalUploadedInGB { get; set; }
        public string LocationOnDisk { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1056:Uri properties should not be strings", Justification = "<Pending>")]
        public string TrackerAnnounceUrl1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1056:Uri properties should not be strings", Justification = "<Pending>")]
        public string TrackerAnnounceUrl2 { get; set; }
    }
}
