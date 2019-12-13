using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TorrentGrease.Hangfire;

namespace TorrentGrease.TorrentStatisticsHarvester
{
    public class TorrentStatisticsHarvesterJob : IJob
    {
        private readonly ILogger<TorrentStatisticsHarvesterJob> _logger;

        public TorrentStatisticsHarvesterJob(ILogger<TorrentStatisticsHarvesterJob> logger)
        {
            _logger = logger;
        }

        public void Execute()
        {
            _logger.LogInformation("Hello world from TorrentStatisticsHarvesterJob");
        }
    }
}
