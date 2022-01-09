using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestUtils.Terminal
{
    public enum ErrorCode
    {
        Unknown,
        CommandError,
        CommandTimeout,
        UnexpectedTerminalExit
    }
}
