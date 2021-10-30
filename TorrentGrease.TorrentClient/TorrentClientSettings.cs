using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TorrentGrease.TorrentClient
{
    public class TorrentClientSettings
    {
        public Uri Url { get; set; } = null;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Client { get; set; } = string.Empty;
        public string TorrentFileDirMapping { get; set; } = string.Empty;
    }
}
