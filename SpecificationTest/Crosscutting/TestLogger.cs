using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SpecificationTest.Crosscutting
{
    public static class TestLogger
    {
        public static void LogDebug(string message)
        {
            Debug.WriteLine(message);
        }
    }
}
