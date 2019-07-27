using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TorrentGrease.Shared
{
    public class Policy
    {
        public int Id { get; set; }
        [Required]
        [StringLength(50, ErrorMessage = "Name is too long.")]
        public string Name { get; set; }
        [Required]
        public int Order { get; set; }
        [Required]
        public bool Enabled { get; set; }
        public ICollection<Condition> Conditions { get; set; }
        public ICollection<Action> Actions { get; set; }
        public ICollection<TrackerPolicy> TrackerPolicies { get; set; }
    }
}
