using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace TorrentGrease.Shared.TorrentClient
{
    [ProtoContract]
    public class Torrent
    {
        [ProtoMember(1)]
        public string Name { get; set; }
        [ProtoMember(2)]
        public Decimal SizeInGB { get; set; }
    }
}
