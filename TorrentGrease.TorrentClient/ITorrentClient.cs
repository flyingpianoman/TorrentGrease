using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TorrentGrease.TorrentClient
{
    public interface ITorrentClient
    {
        Task<IEnumerable<Shared.TorrentClient.Torrent>> GetAllTorrentsAsync();
    }
}
