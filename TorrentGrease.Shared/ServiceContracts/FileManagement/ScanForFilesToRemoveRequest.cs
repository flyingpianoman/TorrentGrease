using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorrentGrease.Shared.ServiceContracts.FileManagement
{
    [ProtoContract]
    public class ScanForFilesToRemoveRequest
    {
        [ProtoMember(1)]
        public IEnumerable<MappedDirectory> CompletedTorrentPathsToScan { get; set; }
        [ProtoMember(2)]
        public long MinFileSizeInBytes { get; set; }
    }
}
