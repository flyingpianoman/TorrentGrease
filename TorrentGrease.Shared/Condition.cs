using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TorrentGrease.Shared
{
    public class Condition
    {
        public int Id { get; set; }
        public int PolicyId { get; set; }
        [Required]
        public int Order { get; set; }
        [Required]
        public LogicalOperator LogicalOperator { get; set; }
        [Required]
        public ConditionType ConditionType { get; set; }
        [Required]
        public string Configuration { get; set; }

    }
}
