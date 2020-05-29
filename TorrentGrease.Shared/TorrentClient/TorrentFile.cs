using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace TorrentGrease.Shared.TorrentClient
{
    [ProtoContract]
    public class TorrentFile
    {
        [ProtoMember(1)]
        public string FileLocationInTorrent { get; set; }
        [ProtoMember(2)]
        public long SizeInBytes { get; set; }
    }
}
