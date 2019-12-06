using ProtoBuf;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace TorrentGrease.Shared
{
    [ProtoContract]
    public class Policy
    {
        [ProtoMember(1)]
        public int Id { get; set; }
        [Required]
        [StringLength(50, ErrorMessage = "Name is too long.")]
        [ProtoMember(2)]
        public string Name { get; set; }
        [StringLength(300, ErrorMessage = "Description is too long.")]
        [ProtoMember(3)]
        public string Description { get; set; }
        [Required]
        [ProtoMember(4)]
        public int Order { get; set; }
        [Required]
        [ProtoMember(5)]
        public bool Enabled { get; set; }
        [ProtoMember(6)]
        public ICollection<Condition> Conditions { get; set; } = new List<Condition>();
        [ProtoMember(7)]
        public ICollection<Action> Actions { get; set; } = new List<Action>();
        [ProtoMember(8)]
        public ICollection<TrackerPolicy> TrackerPolicies { get; set; } = new List<TrackerPolicy>();
        public ICollection<Tracker> Trackers => TrackerPolicies
                    ?.Select(tp => tp.Tracker)
                    ?.ToArray();
    }
}
