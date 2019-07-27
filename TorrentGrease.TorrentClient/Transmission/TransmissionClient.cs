﻿using System;
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
            try
            {
                return (await _rpcClient.TorrentGetAsync(TorrentFields.ALL_FIELDS)
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