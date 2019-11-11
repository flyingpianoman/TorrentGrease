using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TorrentGrease.Shared.ServiceContracts;
using TorrentGrease.Shared.TorrentClient;
using TorrentGrease.TorrentClient;

namespace TorrentGrease.Server.Services
{
    public class TorrentService : ITorrentService
    {
        private readonly ITorrentClient _torrentClient;

        public TorrentService(ITorrentClient torrentClient)
        {
            _torrentClient = torrentClient ?? throw new ArgumentNullException(nameof(torrentClient));
        }

        public async ValueTask<IEnumerable<Torrent>> GetAllTorrents()
        {
            return await _torrentClient.GetAllTorrentsAsync().ConfigureAwait(false);
        }
    }
}
