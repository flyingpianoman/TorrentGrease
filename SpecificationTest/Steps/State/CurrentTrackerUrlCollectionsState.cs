using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TorrentGrease.Shared.TorrentClient;

namespace SpecificationTest.Steps.State
{
    internal class CurrentTrackerUrlCollectionsState
    {
        public IEnumerable<TrackerUrlCollection> LastRetrievedTrackerUrlCollection { get; set; }
    }
}
