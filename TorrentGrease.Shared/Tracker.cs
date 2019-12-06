using ProtoBuf;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;

namespace TorrentGrease.Shared
{
    [ProtoContract]
    public class Tracker
    {
        [ProtoMember(1)]
        public int Id { get; set; }
        [Required]
        [StringLength(50, ErrorMessage = "Name is too long.")]
        [ProtoMember(2)]
        public string Name { get; set; }
        [ProtoMember(3)]
        public ICollection<TrackerPolicy> TrackerPolicies { get; set; } = new List<TrackerPolicy>();
        //[Required]
        //[ProtoMember(4)]
        //public ICollection<string> TrackerUrls { get; set; } //for later
    }
}
