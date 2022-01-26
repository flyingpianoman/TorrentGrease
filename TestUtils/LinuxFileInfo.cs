using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestUtils
{
    public struct LinuxFileInfo
    {
        public long DeviceId { get; init; }
        public long InodeId { get; init; }
        public int HardLinkCount { get; init; }
    }
}
