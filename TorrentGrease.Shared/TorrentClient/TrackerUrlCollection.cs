using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorrentGrease.Shared.TorrentClient
{
    [ProtoContract]
    public class TrackerUrlCollection
    {
        [ProtoMember(1)]
        public List<string> TrackerUrls { get; set; }
    }
}
