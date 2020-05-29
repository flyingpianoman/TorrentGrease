using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TorrentGrease.Shared.ServiceContracts;
using TorrentGrease.Shared.ServiceContracts.TorrentRequests;

namespace TorrentGrease.Client.ServiceClientExtensions
{
    public static class ITorrentServiceExtensions
    {
        public static ValueTask MapTorrentsToDiskAsync(this ITorrentService svc, IEnumerable<string> pathsToScan, IEnumerable<int> torrentIds)
        {
            return svc.MapTorrentsToDiskAsync(new MapTorrentsToDiskRequest
            {
                PathsToScan = pathsToScan,
                TorrentIds = torrentIds
            });
        }
    }
}
