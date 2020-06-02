using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace TorrentGrease.Shared.RelocateTorrent
{
    [ProtoContract]
    public class RelocateTorrentCommand
    {
        [ProtoMember(1)]
        public int TorrentID { get; set; }
        [ProtoMember(2)]
        public string NewLocation { get; set; }
    }
}
