using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestUtils
{
    public struct LinuxFileInfo
    {
        public int FileSystemId { get; init; }
        public int InodeNumber { get; init; }
        public int HardLinkCount { get; init; }
    }
}
