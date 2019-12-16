using System.Collections.Generic;
using System.Threading.Tasks;
using TorrentGrease.Shared.TorrentStatistics;

namespace TorrentGrease.Data.Repositories
{
    public interface ITorrentStatisticsRepository : IRepository
    {
        Task<IList<Torrent>> GetTorrentsByInfoHashAsync(IEnumerable<string> infoHashes);
        Task<IList<Torrent>> GetTorrentsThatWereInLastScanAsync(IEnumerable<int> exlcudedIds);
        Task<Torrent> GetTorrentByInfoHashAsync(string infoHash);
        Task<TorrentUploadDeltaSnapshot> GetLastTorrentUploadDeltaSnapshotByTorrentIdAsync(int torrentId, bool tracking = true);
        Task<IList<TrackerUrlCollection>> GetAllTrackerUrlCollectionsAsync(bool tracking = true);
    }
}