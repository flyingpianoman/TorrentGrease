using ProtoBuf;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;

namespace TorrentGrease.Shared
{
    [ProtoContract]
    public class Action
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
        public ActionType ActionType { get; set; }
        [Required]
        [ProtoMember(5)]
        public string Configuration { get; set; }
    }
}
