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
using System.IO;

namespace TorrentGrease.TorrentClient.Transmission
{
    public class TransmissionClient : ITorrentClient
    {
        private readonly RpcClient.Client _rpcClient;
        private static readonly string[] _torrentFieldsToGet = new[]
            {
                TorrentFields.ID,
                TorrentFields.TOTAL_SIZE, //is in bytes
                TorrentFields.NAME,
                TorrentFields.DOWNLOAD_DIR,
                TorrentFields.HASH_STRING,
                TorrentFields.TRACKERS,
                TorrentFields.UPLOADED_EVER,
                TorrentFields.LEFT_UNTIL_DONE,
                TorrentFields.FILES,
                TorrentFields.ADDED_DATE,
                TorrentFields.ERROR_STRING
            };

        public TransmissionClient(RpcClient.Client rpcClient)
        {
            _rpcClient = rpcClient ?? throw new ArgumentNullException(nameof(rpcClient));
        }

        public async Task<IEnumerable<Torrent>> GetAllTorrentsAsync()
        {
            try
            {
                return (await _rpcClient.TorrentGetAsync(_torrentFieldsToGet)
                    .ConfigureAwait(false))
                    .Torrents
                    .Select(t => t.ToSharedModel())
                    .ToArray();
            }
            catch (NullReferenceException e)
            {
                //The RpcClient doesn't work well in the unhappy flows...
                throw new HttpRequestException("Could not reach transmission", e);
            }
        }
        public async Task<IEnumerable<Torrent>> GetTorrentsByIDsAsync(IEnumerable<int> torrentIDs)
        {
            try
            {
                return (await _rpcClient.TorrentGetAsync(_torrentFieldsToGet, torrentIDs.ToArray())
                    .ConfigureAwait(false))
                    .Torrents
                    .Select(t => t.ToSharedModel())
                    .ToArray();
            }
            catch (NullReferenceException e)
            {
                //The RpcClient doesn't work well in the unhappy flows...
                throw new HttpRequestException("Could not reach transmission", e);
            }
        }

        public async Task AddTorrentAsync(string torrentName, string torrentFile, string downloadDir, int nrOfFilesToInclude)
        {
            downloadDir = downloadDir.Replace('\\', '/');

            var torrent = new NewTorrent
            {
                Metainfo = Convert.ToBase64String(File.ReadAllBytes(torrentFile)),
                DownloadDirectory = downloadDir,
                FilesWanted = Enumerable.Range(0, nrOfFilesToInclude).ToArray()
            };

            await _rpcClient.TorrentAddAsync(torrent);
        }

        public Task RemoveTorrentsByIDsAsync(IEnumerable<int> IDs, bool deleteData)
        {
            _rpcClient.TorrentRemove(IDs.ToArray(), deleteData);
            return Task.CompletedTask;
        }

        public Task RelocateTorrentAsync(int ID, string newLocation, bool moveDataFromOldLocation = false)
        {
            _rpcClient.TorrentSetLocation(new int[] { ID }, newLocation, moveDataFromOldLocation);
            return Task.CompletedTask;
        }

        public Task VerifyTorrentsAsync(int[] IDs)
        {
            _rpcClient.TorrentVerify(IDs.Cast<object>().ToArray());
            return Task.CompletedTask;
        }
    }
}