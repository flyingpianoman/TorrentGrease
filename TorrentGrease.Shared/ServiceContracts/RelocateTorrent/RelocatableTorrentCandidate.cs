using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace TorrentGrease.Shared.ServiceContracts.RelocateTorrent
{
    [ProtoContract]
    public class RelocatableTorrentCandidate
    {
        [ProtoMember(1)]
        public int TorrentID { get; set; }
        [ProtoMember(2)]
        public string TorrentName { get; set; }
        [ProtoMember(3)]
        public ICollection<string> TorrentFilePaths { get; set; } = new List<string>();
        [ProtoMember(4)]
        public ICollection<string> RelocateOptions { get; set; } = new List<string>();
        [ProtoMember(5)]
        public string ChosenOption { get; set; }


    }
}
