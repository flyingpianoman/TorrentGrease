using SpecificationTest.Crosscutting;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpecificationTest
{
    internal static class TestSettings
    {
        public static string SeleniumHubAddress => $"http://{DockerHelper.DockerHostAddress}:4444/wd/hub";
        public static string TorrentGreaseDockerAddress => "http://torrent-grease:5656";
        public static string TorrentGreaseExposedAddress => $"http://{DockerHelper.DockerHostAddress}:5656";

        public static string TransmissionExposedAddress => $"http://{DockerHelper.DockerHostAddress}:9091";
        public static string TransmissionUser => "usr";
        public static string TransmissionPassword => "pwd";
    }
}
