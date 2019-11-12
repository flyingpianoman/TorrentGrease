using ProtoBuf;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace TorrentGrease.Shared.ServiceContracts
{
    [ServiceContract]
    public interface ITorrentService
    {
        ValueTask<IEnumerable<Shared.TorrentClient.Torrent>> GetAllTorrentsAsync();
    }
}
