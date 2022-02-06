using ProtoBuf;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using TorrentGrease.Shared.ServiceContracts.ReaddTorrents;
using TorrentGrease.Shared.ServiceContracts.RelocateTorrent;
using TorrentGrease.Shared.ServiceContracts.TorrentRequests;
using TorrentGrease.Shared.TorrentClient;

namespace TorrentGrease.Shared.ServiceContracts
{
    [ServiceContract]
    public interface ITorrentService
    {
        ValueTask<IEnumerable<Shared.TorrentClient.Torrent>> GetAllTorrentsAsync();
        Task<List<RelocatableTorrentCandidate>> FindRelocatableTorrentCandidatesAsync(MapTorrentsToDiskRequest request);
        Task RelocateTorrentsAsync(RelocateTorrentsRequest request);
        Task ReaddTorrentsAsync(ReaddTorrentsRequest request);
        ValueTask<IEnumerable<TrackerUrlCollection>> GetCurrentTrackerUrlCollectionsAsync();
    }
}
