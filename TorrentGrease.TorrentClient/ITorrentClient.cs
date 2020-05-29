using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TorrentGrease.TorrentClient
{
    public interface ITorrentClient
    {
        Task<IEnumerable<Shared.TorrentClient.Torrent>> GetAllTorrentsAsync();
        Task<IEnumerable<Shared.TorrentClient.Torrent>> GetTorrentsByIDsAsync(IEnumerable<int> torrentIDs);

        Task AddTorrentAsync(string torrentName, string torrentFile, string downloadDir, int nrOfFilesToInclude);
        Task RemoveTorrentsByIDsAsync(IEnumerable<int> IDs, bool deleteData);
    }
}
