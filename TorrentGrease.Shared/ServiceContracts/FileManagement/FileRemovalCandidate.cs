using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorrentGrease.Shared.ServiceContracts.FileManagement
{
    [ProtoContract]
    public class FileRemovalCandidate
    {
        [ProtoMember(1)]
        public string FilePath { get; set; }
        [ProtoMember(2)]
        public long FileSizeInBytes { get; set; }
    }
}
