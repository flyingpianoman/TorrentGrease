using SpecificationTest.Crosscutting;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpecificationTest
{
    internal static class TestSettings
    {
        public static string SeleniumHubAddress => $"http://localhost:4444/wd/hub";
        public static string TorrentGreaseDockerAddress => "http://torrent-grease:5656";
        public static string TorrentGreaseExposedAddress => $"http://localhost:5656";

        public static string TransmissionExposedAddress => $"http://localhost:9091";
        public static string TransmissionUser => "usr";
        public static string TransmissionPassword => "pwd";
        public static string TorrentGreaseContainerName => "torrent-grease";
    }
}
