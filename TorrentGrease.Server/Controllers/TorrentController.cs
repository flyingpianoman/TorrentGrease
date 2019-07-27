using TorrentGrease.Shared;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TorrentGrease.TorrentClient;

namespace TorrentGrease.Server.Controllers
{
    [Route("api/[controller]")]
    public class TorrentController : Controller
    {
        private readonly ITorrentClient _torrentClient;

        public TorrentController(ITorrentClient torrentClient)
        {
            _torrentClient = torrentClient ?? throw new ArgumentNullException(nameof(torrentClient));
        }

        [HttpGet("[action]")]
        public async Task<IEnumerable<Shared.TorrentClient.Torrent>> All()
        {
            return await _torrentClient.GetAllTorrentsAsync().ConfigureAwait(false);
        }
    }
}
