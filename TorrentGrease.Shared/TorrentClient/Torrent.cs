using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace TorrentGrease.Shared.TorrentClient
{
    [ProtoContract]
    public class Torrent
    {
        [ProtoMember(1)]
        public string Name { get; set; }
        [ProtoMember(2)]
        public long SizeInBytes { get; set; }
        public decimal SizeInGB => (new Decimal(SizeInBytes) / 1024 / 1024 / 1024);
        [ProtoMember(3)]
        public string InfoHash { get; set; }
        [ProtoMember(4)]
        public string Location { get; set; }
        [ProtoMember(5)]
        public long BytesOnDisk { get; set; }
        public decimal GBsOnDisk => (new Decimal(BytesOnDisk) / 1024 / 1024 / 1024);
        [ProtoMember(6)]
        public long TotalUploadInBytes { get; set; }
        public decimal TotalUploadInGB => (new Decimal(TotalUploadInBytes) / 1024 / 1024 / 1024);
        [ProtoMember(7)]
        public List<string> TrackerUrls { get; set; }
        [ProtoMember(8)]
        public DateTime AddedDateTime { get; set; }
    }
}
