using System;
using System.Collections.Generic;
using System.Text;

namespace TestUtils
{
    public static class TestSettings
    {
        public static Uri SeleniumHubAddress => new Uri("http://localhost:4444");
        public static Uri TorrentGreaseDockerAddress => new Uri("http://torrent-grease:5656");
        public static Uri TorrentGreaseExposedAddress => new Uri("http://localhost:5656");

        public static Uri TransmissionExposedAddress => new Uri("http://localhost:9091");
        public static string TransmissionUser => "usr";
        public static string TransmissionPassword => "pwd";
        public static string TorrentGreaseContainerName => "torrent-grease";

        public static string TransmissionContainerName => "transmission";
    }
}
