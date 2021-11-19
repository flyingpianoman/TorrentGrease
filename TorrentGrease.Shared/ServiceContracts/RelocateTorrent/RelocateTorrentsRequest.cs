using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace TorrentGrease.Shared.ServiceContracts.RelocateTorrent
{
    [ProtoContract]
    public class RelocateTorrentsRequest
    {
        [ProtoMember(1)]
        public IList<RelocateTorrentCommand> RelocateTorrentCommands { get; set; }
        [ProtoMember(2)]
        public bool VerifyAfterMoving { get; set; }
    }
}
