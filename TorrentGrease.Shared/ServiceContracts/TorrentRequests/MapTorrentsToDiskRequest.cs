using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace TorrentGrease.Shared.ServiceContracts.TorrentRequests
{
    [ProtoContract]
    public class MapTorrentsToDiskRequest
    {
        [ProtoMember(1)]
        public IEnumerable<string> PathsToScan { get; set; }
        [ProtoMember(2)]
        public IEnumerable<int> TorrentIds { get; set; }
    }
}
