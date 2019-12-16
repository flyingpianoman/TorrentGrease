using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TorrentGrease.Shared.TorrentClient;
using RpcClient = Transmission.API.RPC;
using Transmission.API.RPC.Entity;
using TorrentGrease.TorrentClient;
using TorrentGrease.TorrentClient.Transmission;
using System.Net;
using System.Net.Http;

namespace TorrentGrease.TorrentClient.Transmission
{
    public class TransmissionClient : ITorrentClient
    {
        private readonly RpcClient.Client _rpcClient;

        public TransmissionClient(RpcClient.Client rpcClient)
        {
            _rpcClient = rpcClient ?? throw new ArgumentNullException(nameof(rpcClient));
        }

        public async Task<IEnumerable<Torrent>> GetAllTorrentsAsync()
        {
            var fields = new[]
            {
                TorrentFields.TOTAL_SIZE, //is in bytes
                TorrentFields.NAME,
                TorrentFields.DOWNLOAD_DIR,
                TorrentFields.HASH_STRING,
                TorrentFields.TRACKERS,
                TorrentFields.UPLOADED_EVER,
                TorrentFields.LEFT_UNTIL_DONE,
                TorrentFields.FILES,
                TorrentFields.ADDED_DATE
            };

            try
            {
                return (await _rpcClient.TorrentGetAsync(fields)
                    .ConfigureAwait(false))
                    .Torrents
                    .Select(t => t.ToSharedModel())
                    .ToArray();
            }
            catch(NullReferenceException e)
            {
                //The RpcClient doesn't work well in the unhappy flows...
                throw new HttpRequestException("Could not reach transmission", e);
            }
        }
    }
}