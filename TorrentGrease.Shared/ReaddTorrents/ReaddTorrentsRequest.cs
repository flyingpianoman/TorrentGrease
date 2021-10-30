using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace TorrentGrease.Shared.ReaddTorrents
{
    [ProtoContract]
    public class ReaddTorrentsRequest
    {
        [ProtoMember(1)]
        public IList<int> TorrentIDs { get; set; }
    }
}
