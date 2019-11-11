using ProtoBuf;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;

namespace TorrentGrease.Shared
{
    [ProtoContract]
    public class Condition
    {
        [ProtoMember(1)]
        public int Id { get; set; }
        [ProtoMember(2)]
        public int PolicyId { get; set; }
        [Required]
        [ProtoMember(3)]
        public int Order { get; set; }
        [Required]
        [ProtoMember(4)]
        public LogicalOperator LogicalOperator { get; set; }
        [Required]
        [ProtoMember(5)]
        public ConditionType ConditionType { get; set; }
        [Required]
        [ProtoMember(6)]
        public string Configuration { get; set; }

    }
}
