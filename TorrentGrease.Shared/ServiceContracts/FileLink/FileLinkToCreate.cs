using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorrentGrease.Shared.ServiceContracts.FileLink
{
    [ProtoContract]
    public class FileLinkToCreate
    {
        [ProtoMember(1)]
        public IEnumerable<string> FilePaths { get; set; }
    }
}
