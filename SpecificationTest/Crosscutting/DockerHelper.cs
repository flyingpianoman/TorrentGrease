using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SpecificationTest.Crosscutting
{
    internal static class DockerHelper
    {
        public static bool IsRunningDockerToolbox => Directory.Exists(@"C:\Program Files\Docker Toolbox");
        public static string DockerHostAddress => IsRunningDockerToolbox ? "192.168.99.100" : "localhost";
    }
}
