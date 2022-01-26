using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecificationTest.Steps.Models
{
    public class FileLinkCandidateDto
    {
        public string FilePath1 { get; set; }
        public string FilePath2 { get; set; }
        public string FilePath3 { get; set; }
        public IEnumerable<string> FilePaths => new string[] { FilePath1, FilePath2, FilePath3 }.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
        public string Size { get; set; }
    }
}
