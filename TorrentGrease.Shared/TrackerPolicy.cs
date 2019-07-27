using System;
using System.Collections.Generic;
using System.Text;

namespace TorrentGrease.Shared
{
    public class TrackerPolicy
    {
        public TrackerPolicy(int trackerId, int policyId)
        {
            TrackerId = trackerId;
            PolicyId = policyId;
        }

        public int TrackerId { get; set; }
        public Tracker Tracker { get; set; }
        public int PolicyId { get; set; }
        public Policy Policy { get; set; }
    }
}
