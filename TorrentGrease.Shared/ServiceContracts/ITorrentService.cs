using ProtoBuf;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using TorrentGrease.Shared.ServiceContracts.TorrentRequests;

namespace TorrentGrease.Shared.ServiceContracts
{
    [ServiceContract]
    public interface ITorrentService
    {
        ValueTask<IEnumerable<Shared.TorrentClient.Torrent>> GetAllTorrentsAsync();
        ValueTask MapTorrentsToDiskAsync(MapTorrentsToDiskRequest request);
    }
}
