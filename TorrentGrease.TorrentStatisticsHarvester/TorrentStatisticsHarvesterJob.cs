using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TorrentGrease.Hangfire;
using TorrentGrease.TorrentClient;

namespace TorrentGrease.TorrentStatisticsHarvester
{
    public class TorrentStatisticsHarvesterJob : IAsyncJob
    {
        private readonly ILogger<TorrentStatisticsHarvesterJob> _logger;
        private readonly ITorrentClient _torrentClient;

        public TorrentStatisticsHarvesterJob(ILogger<TorrentStatisticsHarvesterJob> logger, ITorrentClient torrentClient)
        {
            _logger = logger;
            _torrentClient = torrentClient;
        }

        public async Task ExecuteAsync()
        {
            var torrents = await _torrentClient.GetAllTorrentsAsync().ConfigureAwait(false);
        }
    }
}
