using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorrentGrease.Shared.ServiceContracts.FileLink
{
    [ProtoContract]
    public class ScanForPossibleFileLinksRequest
    {
        [ProtoMember(1)]
        public IEnumerable<string> PathsToScan { get; set; }
        [ProtoMember(2)]
        public long MinFileSizeInBytes { get; set; }
        [ProtoMember(3)]
        public long FullByteComparisonMaxFileSize { get; set; }

    }
}
