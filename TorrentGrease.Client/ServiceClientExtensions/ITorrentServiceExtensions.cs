﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TorrentGrease.Shared.ServiceContracts.RelocateTorrent;
using TorrentGrease.Shared.ServiceContracts;
using TorrentGrease.Shared.ServiceContracts.TorrentRequests;

namespace TorrentGrease.Client.ServiceClientExtensions
{
    public static class ITorrentServiceExtensions
    {
        public static Task<List<RelocatableTorrentCandidate>> FindRelocatableTorrentCandidatesAsync(this ITorrentService svc, IEnumerable<string> pathsToScan, IEnumerable<int> torrentIds)
        {
            return svc.FindRelocatableTorrentCandidatesAsync(new MapTorrentsToDiskRequest
            {
                PathsToScan = pathsToScan,
                TorrentIds = torrentIds
            });
        }

        public static Task RelocateTorrentsAsync(this ITorrentService svc, 
            IList<RelocateTorrentCommand> relocateTorrentCommands, bool verifyAfterMoving)
        {
            return svc.RelocateTorrentsAsync(new RelocateTorrentsRequest
            {
                RelocateTorrentCommands = relocateTorrentCommands,
                VerifyAfterMoving = verifyAfterMoving
            });
        }
    }
}
