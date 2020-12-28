using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TorrentGrease.Shared.TorrentStatistics;

namespace TorrentGrease.Data.Repositories
{
    public class TorrentStatisticsRepository : RepositoryBase, ITorrentStatisticsRepository
    {
        public TorrentStatisticsRepository(ITorrentGreaseDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IList<TrackerUrlCollection>> GetAllTrackerUrlCollectionsAsync(bool tracking = true)
        {
            var source = tracking
                ? _dbContext.TrackerUrlCollections
                : _dbContext.TrackerUrlCollections.AsNoTracking();

            return await source
                .Include(s => s.TrackerUrls)
                .ToListAsync();
        }

        public async Task<TorrentUploadDeltaSnapshot> GetLastTorrentUploadDeltaSnapshotByTorrentIdAsync(int torrentId, bool tracking = true)
        {
            var source = tracking
                ? _dbContext.TorrentUploadDeltaSnapshots
                : _dbContext.TorrentUploadDeltaSnapshots.AsNoTracking();

            return await source
                .Where(x => x.TorrentId == torrentId)
                .OrderByDescending(x => x.DateTime)
                .SingleOrDefaultAsync();
        }

        public async Task<Torrent> GetTorrentByInfoHashAsync(string infoHash)
        {
            return (await GetTorrentsByInfoHashAsync(new[] { infoHash })).Single();
        }

        public async Task<IList<Torrent>> GetTorrentsByInfoHashAsync(IEnumerable<string> infoHashes)
        {
            return await _dbContext.Torrents
                .Where(t => infoHashes.Contains(t.InfoHash))
                .ToListAsync();
        }

        public async Task<IList<Torrent>> GetTorrentsThatWereInLastScanAsync(IEnumerable<int> exlcudedIds)
        {
            return await _dbContext.Torrents
                .Where(t => !exlcudedIds.Contains(t.Id) && t.WasInClientOnLastScan)
                .ToListAsync();
        }
    }
}
