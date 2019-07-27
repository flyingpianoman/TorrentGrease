using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TorrentGrease.Shared
{
    public class Action
    {
        public int Id { get; set; }
        [Required]
        [StringLength(50, ErrorMessage = "Name is too long.")]
        public string Name { get; set; }
        [Required]
        public int Order { get; set; }
        [Required]
        public ActionType ActionType { get; set; }
        [Required]
        public string Configuration { get; set; }
    }
}
