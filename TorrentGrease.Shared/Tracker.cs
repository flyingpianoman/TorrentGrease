using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TorrentGrease.Shared
{
    public class Tracker
    {
        public Tracker(int id, string name)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public int Id { get; set; }
        [Required]
        [StringLength(50, ErrorMessage = "Name is too long.")]
        public string Name { get; set; }
        [Required]
        public ICollection<string> TrackerUrls { get; set; }
        public ICollection<TrackerPolicy> TrackerPolicies { get; set; }
    }
}
