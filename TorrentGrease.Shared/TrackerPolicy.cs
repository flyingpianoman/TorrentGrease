using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace TorrentGrease.Shared
{
    [ProtoContract]
    public class TrackerPolicy
    {
        [ProtoMember(1)]
        public int TrackerId { get; set; }
        [ProtoMember(2)]
        public Tracker Tracker { get; set; }
        [ProtoMember(3)]
        public int PolicyId { get; set; }
        [ProtoMember(4)]
        public Policy Policy { get; set; }
    }
}
