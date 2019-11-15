using ProtoBuf;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        [Required]
        [ProtoMember(3)]
        public int Order { get; set; }
        [Required]
        [ProtoMember(4)]
        public bool Enabled { get; set; } //TODO move this TrackerPolicy
        [ProtoMember(5)]
        public ICollection<Condition> Conditions { get; set; }
        [ProtoMember(6)]
        public ICollection<Action> Actions { get; set; }
        [ProtoMember(7)]
        public ICollection<TrackerPolicy> TrackerPolicies { get; set; }
    }
}
