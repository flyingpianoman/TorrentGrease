using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TorrentGrease.Hangfire;
using TorrentGrease.TorrentClient;
using TorrentGrease.Data.Repositories;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography;
using Stats = TorrentGrease.Shared.TorrentStatistics;

namespace TorrentGrease.TorrentStatisticsHarvester
{
    public sealed class TorrentStatisticsHarvesterJob : IAsyncJob, IDisposable
    {
        private readonly ILogger<TorrentStatisticsHarvesterJob> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ITorrentClient _torrentClient;
        private readonly SHA256 _hasher;

        public TorrentStatisticsHarvesterJob(ILogger<TorrentStatisticsHarvesterJob> logger,
            IServiceScopeFactory serviceScopeFactory, ITorrentClient torrentClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            _torrentClient = torrentClient ?? throw new ArgumentNullException(nameof(torrentClient));
            _hasher = SHA256.Create();
        }

        public async Task ExecuteAsync()
        {
            var jobDateTime = DateTime.UtcNow;
            _logger.LogInformation("Adding new snapshots for torrents");
            var torrentsFromClient = await _torrentClient.GetAllTorrentsAsync().ConfigureAwait(false);
            _logger.LogInformation("Found {0} torrents in client", torrentsFromClient.Count());

            await SynchronizeTorrentDbAsync(torrentsFromClient);
            await AddUploadDeltaSnapshotsAsync(jobDateTime, torrentsFromClient);
        }

        #region SynchronizeTorrentDbAsync
        /// <summary>
        /// Synchronize persisted torrents with torrents in torrent client
        /// </summary>
        private async Task SynchronizeTorrentDbAsync(IEnumerable<Shared.TorrentClient.Torrent> torrentsFromClient)
        {
            _logger.LogInformation("Synchronizing torrents in the torrentclient with the torrents in the db");
            var infoHashes = torrentsFromClient.Select(t => t.InfoHash).ToArray();

            //Use a scoped repo so that we'll dispose the dbcontext after we're done with this step freeing up memory
            using var scope = _serviceScopeFactory.CreateScope();
            var torrentStatisticsRepository = scope.ServiceProvider.GetRequiredService<ITorrentStatisticsRepository>();

            _logger.LogInformation("Retrieving persisted torrents for torrents currently in torrentclient");
            var persistedTorrents = await torrentStatisticsRepository.GetTorrentsByInfoHashAsync(infoHashes);

            UpdateReAddedTorrents(persistedTorrents);
            await AddNewTorrentsAsync(torrentStatisticsRepository, torrentsFromClient, persistedTorrents);
            await UpdateRemovedTorrentsAsync(torrentStatisticsRepository, persistedTorrents);

            _logger.LogInformation("Send changes to db");
            await torrentStatisticsRepository.SaveChangesAsync();
        }

        private void UpdateReAddedTorrents(IList<Stats.Torrent> persistedTorrents)
        {
            var reAddedTorrents = persistedTorrents.Where(t => !t.WasInClientOnLastScan).ToArray();
            _logger.LogInformation("Updating {0} torrents that were re-added in the torrentclient", reAddedTorrents.Length);

            foreach (var torrent in reAddedTorrents)
            {
                _logger.LogDebug("Setting WasInClientOnLastScan to true for torrent with hash {0}", torrent.InfoHash);
                torrent.WasInClientOnLastScan = true;
            }
        }

        private async Task AddNewTorrentsAsync(ITorrentStatisticsRepository torrentStatisticsRepository,
            IEnumerable<Shared.TorrentClient.Torrent> torrentsFromClient, IList<Stats.Torrent> persistedTorrents)
        {
            var newTorrents = torrentsFromClient
                .Where(t => !persistedTorrents.Any(pt => pt.InfoHash == t.InfoHash))
                .Select(t => new Stats.Torrent(t))
                .ToArray();

            _logger.LogInformation("Adding {0} new torrents that were added to the torrentclient for the first time", newTorrents.Length);

            foreach (var newTorrent in newTorrents)
            {
                _logger.LogDebug("Adding torrent with hash {0} and name '{1}'", newTorrent.InfoHash, newTorrent.Name);
            }

            await torrentStatisticsRepository.AddRangeAsync(newTorrents);
        }

        /// <summary>
        /// Disable torrents that are missing since last scan
        /// </summary>
        private async Task UpdateRemovedTorrentsAsync(ITorrentStatisticsRepository torrentStatisticsRepository, IList<Shared.TorrentStatistics.Torrent> persistedTorrents)
        {
            var exlcudedIds = persistedTorrents.Select(t => t.Id).ToArray();
            var removedTorrents = await torrentStatisticsRepository.GetTorrentsThatWereInLastScanAsync(exlcudedIds);
            _logger.LogInformation("Updating {0} torrents that were removed in the torrentclient", removedTorrents.Count);

            foreach (var removedTorrent in removedTorrents)
            {
                _logger.LogDebug("Setting WasInClientOnLastScan to false for torrent with hash {0} and name '{1}'", removedTorrent.InfoHash, removedTorrent.Name);
                removedTorrent.WasInClientOnLastScan = false;
            }
        }
        #endregion

        #region AddUploadDeltaSnapshotsAsync
        private async Task AddUploadDeltaSnapshotsAsync(DateTime jobDateTime, IEnumerable<Shared.TorrentClient.Torrent> torrentsFromClient)
        {
            _logger.LogInformation("Adding new upload delta snapshots for any relevant changes");

            //Use a scoped repo so that we'll dispose the dbcontext after we're done with this step freeing up memory
            using var scope = _serviceScopeFactory.CreateScope();
            var torrentStatisticsRepository = scope.ServiceProvider.GetRequiredService<ITorrentStatisticsRepository>();

            _logger.LogDebug("Retrieve existing tracker url collections, we're most likely going to reuse them in the new deltas");
            var trackerUrlCollectionsDict = (await torrentStatisticsRepository.GetAllTrackerUrlCollectionsAsync(tracking: false))
                .ToDictionary(tuc => tuc.CollectionHash);

            await AddUploadDeltaSnapshotsInnerAsync(jobDateTime, torrentsFromClient, torrentStatisticsRepository, trackerUrlCollectionsDict);

            _logger.LogInformation("Send changes to db");
            await torrentStatisticsRepository.SaveChangesAsync();
        }

        private async Task AddUploadDeltaSnapshotsInnerAsync(DateTime jobDateTime, IEnumerable<Shared.TorrentClient.Torrent> torrentsFromClient, 
            ITorrentStatisticsRepository torrentStatisticsRepository, Dictionary<byte[], Stats.TrackerUrlCollection> trackerUrlCollectionsDict)
        {
            foreach (var clientTorrent in torrentsFromClient)
            {
                _logger.LogDebug("Retrieving torrentdata and last snapshot for torrent with hash {0}", clientTorrent.InfoHash);
                var persistedTorrent = await torrentStatisticsRepository.GetTorrentByInfoHashAsync(clientTorrent.InfoHash);
                var lastUploadDeltaSnapshot = await torrentStatisticsRepository.GetLastTorrentUploadDeltaSnapshotByTorrentIdAsync(persistedTorrent.Id, tracking: true);

                _logger.LogDebug("Retrieving or creating tracker url collection for torrent with hash {0}", clientTorrent.InfoHash);
                var trackerUrlCollection = await GetOrCreateTrackerUrlCollectionAsync(torrentStatisticsRepository, trackerUrlCollectionsDict, clientTorrent);

                //Returns null if there's no significant change to create a delta for
                var newUploadDeltaSnapshot = CreateNewUploadDeltaSnapshot(jobDateTime, clientTorrent, persistedTorrent, lastUploadDeltaSnapshot, trackerUrlCollection);

                if (newUploadDeltaSnapshot != null)
                {
                    await torrentStatisticsRepository.AddAsync(newUploadDeltaSnapshot);
                }
            }
        }

        #region CreateNewUploadDeltaSnapshot
        /// <summary>
        /// Returns null if nothing relevant has changed
        /// </summary>
        private Stats.TorrentUploadDeltaSnapshot CreateNewUploadDeltaSnapshot(DateTime jobDateTime, Shared.TorrentClient.Torrent clientTorrent, Stats.Torrent persistedTorrent, Stats.TorrentUploadDeltaSnapshot lastUploadDeltaSnapshot, Stats.TrackerUrlCollection trackerUrlCollection)
        {
            _logger.LogDebug("Creating new upload delta snapshot for torrent with hash {0} and name '{1}' if there are any relevant changes", clientTorrent.InfoHash, clientTorrent.Name);

            if (lastUploadDeltaSnapshot == null)
            {
                return CreateUploadDeltaSnapshotForNewTorrent(jobDateTime, clientTorrent, persistedTorrent, trackerUrlCollection);
            }

            if (clientTorrent.AddedDateTime != persistedTorrent.LatestAddedDateTime)
            {
                return CreateNewUploadDeltaSnapshotForReAddedTorrent(jobDateTime, clientTorrent, persistedTorrent, lastUploadDeltaSnapshot, trackerUrlCollection);
            }
            else
            {
                return CreateUploadDeltaSnapshotForExistingTorrent(jobDateTime, clientTorrent, persistedTorrent, lastUploadDeltaSnapshot, trackerUrlCollection);
            }
        }

        private Stats.TorrentUploadDeltaSnapshot CreateUploadDeltaSnapshotForNewTorrent(DateTime jobDateTime, Shared.TorrentClient.Torrent clientTorrent,
            Stats.Torrent persistedTorrent, Stats.TrackerUrlCollection trackerUrlCollection)
        {
            _logger.LogDebug("Creating first upload delta snapshot for torrent with hash {0}", clientTorrent.InfoHash);

            return CreateTorrentUploadDeltaSnapshot(jobDateTime, clientTorrent, persistedTorrent, trackerUrlCollection,
                delta: clientTorrent.TotalUploadInBytes, totalUpload: clientTorrent.TotalUploadInBytes);
        }
        private Stats.TorrentUploadDeltaSnapshot CreateNewUploadDeltaSnapshotForReAddedTorrent(DateTime jobDateTime, Shared.TorrentClient.Torrent clientTorrent, Stats.Torrent persistedTorrent, Stats.TorrentUploadDeltaSnapshot lastUploadDeltaSnapshot, Stats.TrackerUrlCollection trackerUrlCollection)
        {
            _logger.LogInformation("Torrent with hash {0} seems the be re-added", clientTorrent.InfoHash);
            persistedTorrent.LatestAddedDateTime = clientTorrent.AddedDateTime;

            var delta = clientTorrent.TotalUploadInBytes;
            var totalUpload = lastUploadDeltaSnapshot.TotalUploadInBytes + delta;

            return CreateTorrentUploadDeltaSnapshot(jobDateTime, clientTorrent, persistedTorrent, trackerUrlCollection, delta, totalUpload);
        }

        private Stats.TorrentUploadDeltaSnapshot CreateUploadDeltaSnapshotForExistingTorrent(DateTime jobDateTime, Shared.TorrentClient.Torrent clientTorrent, Stats.Torrent persistedTorrent, Stats.TorrentUploadDeltaSnapshot lastUploadDeltaSnapshot, Stats.TrackerUrlCollection trackerUrlCollection)
        {
            var delta = clientTorrent.TotalUploadInBytes - lastUploadDeltaSnapshot.TotalUploadForThisTorrentInBytes;

            if (delta == 0)
            {
                _logger.LogDebug("Torrent with hash {0} has a delta of 0, skipping", clientTorrent.InfoHash);
                return null;
            }

            if (delta < 0)
            {
                _logger.LogWarning("Upload delta for torrent with hash {0} is {1} which shouldn't happen, this could be a bug or mean that the torrent client db was restored. Truncating to 0", clientTorrent.InfoHash, delta);
                delta = 0;
            }

            var totalUpload = lastUploadDeltaSnapshot.TotalUploadInBytes + delta;
            return CreateTorrentUploadDeltaSnapshot(jobDateTime, clientTorrent, persistedTorrent, trackerUrlCollection, delta, totalUpload);
        }

        private static Stats.TorrentUploadDeltaSnapshot CreateTorrentUploadDeltaSnapshot(DateTime jobDateTime, Shared.TorrentClient.Torrent clientTorrent, Stats.Torrent persistedTorrent, Stats.TrackerUrlCollection trackerUrlCollection, long delta, long totalUpload)
        {
            return new Stats.TorrentUploadDeltaSnapshot
            {
                DateTime = jobDateTime,
                TorrentId = persistedTorrent.Id,
                TotalUploadForThisTorrentInBytes = clientTorrent.TotalUploadInBytes,
                TrackerUrlCollection = trackerUrlCollection,

                TotalUploadInBytes = totalUpload,
                UploadDeltaSinceLastSnapshotInBytes = delta,
            };
        }
        #endregion

        #region GetOrCreateTrackerUrlCollectionAsync
        private async ValueTask<Stats.TrackerUrlCollection> GetOrCreateTrackerUrlCollectionAsync
            (ITorrentStatisticsRepository torrentStatisticsRepository,
            Dictionary<byte[], Stats.TrackerUrlCollection> trackerUrlCollectionsDict,
            Shared.TorrentClient.Torrent clientTorrent)
        {
            var trackerUrls = clientTorrent.TrackerUrls
                                        .Select(tu => tu.ToLower())
                                        .OrderBy(tu => tu)
                                        .Distinct()
                                        .ToArray();

            var hashString = string.Join(";", trackerUrls);
            var hash = _hasher.ComputeHash(Encoding.UTF8.GetBytes(hashString));
            _logger.LogTrace("Retrieving or creating tracker url collection for torrent with hash {0}", clientTorrent.InfoHash);

            if (!trackerUrlCollectionsDict.TryGetValue(hash, out var trackerUrlCollection))
            {
                _logger.LogDebug("Didn't find any tracker url collection for torrent with hash {0}, creating one (urls are: {1})", clientTorrent.InfoHash, trackerUrls);

                trackerUrlCollection = new Stats.TrackerUrlCollection()
                {
                    CollectionHash = hash,
                    TrackerUrls = trackerUrls.Select(tu => new Stats.TrackerUrl(tu)).ToList()
                };
                await torrentStatisticsRepository.AddAsync(trackerUrlCollection);
                trackerUrlCollectionsDict[hash] = trackerUrlCollection;
            }
            else
            {
                _logger.LogTrace("Found tracker url collection for torrent with hash {0}", clientTorrent.InfoHash);
            }

            return trackerUrlCollection;
        }
        #endregion
        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _hasher.Dispose();
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
