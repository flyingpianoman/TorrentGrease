using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorrentGrease.Shared.ServiceContracts.FileManagement
{
    [ProtoContract]
    public class MappedDirectory
    {
        [ProtoMember(1)]
        public string TorrentClientDir { get; set; }
        [ProtoMember(2)]
        public string TorrentGreaseDir { get; set; }
    }
}
