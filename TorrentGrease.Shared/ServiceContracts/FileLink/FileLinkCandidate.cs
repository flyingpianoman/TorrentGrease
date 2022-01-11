using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorrentGrease.Shared.ServiceContracts.FileLink
{
    [ProtoContract]
    public class FileLinkCandidate
    {
        [ProtoMember(1)]
        public List<string> FilePaths { get; set; }
        [ProtoMember(2)]
        public long FileSizeInBytes { get; set; }
    }
}
