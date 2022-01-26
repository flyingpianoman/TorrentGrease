using ProtoBuf;

namespace TorrentGrease.Shared.ServiceContracts.FileLink
{
    [ProtoContract]
    public class FileLinkCandidateFile
    {
        [ProtoMember(1)]
        public string FilePath { get; set; }
        [ProtoMember(2)]
        public long DeviceId { get; set; }
        [ProtoMember(3)]
        public long InodeId { get; set; }
    }
}