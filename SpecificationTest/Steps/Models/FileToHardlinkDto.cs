using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecificationTest.Steps.Models
{
    public class FileToHardlinkDto
    {
        public string FilePath { get; set; }
        public string HardLinkTargetPath { get; set; }
    }
}
