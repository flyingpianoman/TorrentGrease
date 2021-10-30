using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TorrentGrease.Shared.TorrentClient;

namespace TorrentGrease.TorrentClient
{
    public interface ITorrentClient
    {
        Task<IEnumerable<Shared.TorrentClient.Torrent>> GetAllTorrentsAsync();
        Task<IEnumerable<Shared.TorrentClient.Torrent>> GetTorrentsByIDsAsync(IEnumerable<int> torrentIDs);

        Task AddTorrentAsync(string torrentName, string torrentFile, string downloadDir, int nrOfFilesToInclude);
        Task ReAddTorrentAsync(Stream torrentFileStream, int torrentId);
        Task RemoveTorrentsByIDsAsync(IEnumerable<int> IDs, bool deleteData);
        Task RelocateTorrentAsync(int ID, string newLocation, bool moveDataFromOldLocation = false);
        Task VerifyTorrentsAsync(int[] IDs);
        Task<Stream> DownloadTorrentFileAsync(Torrent torrent);
    }
}
