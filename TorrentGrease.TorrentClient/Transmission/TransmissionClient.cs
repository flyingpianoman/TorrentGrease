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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TorrentGrease.TorrentClient.Transmission
{
    public class TransmissionClient : ITorrentClient
    {
        private readonly RpcClient.Client _rpcClient;
        private readonly IOptions<TorrentClientSettings> _settings;
        private readonly ILogger<TransmissionClient> _logger;
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
                TorrentFields.ERROR_STRING,
                TorrentFields.TORRENT_FILE
            };

        public TransmissionClient(RpcClient.Client rpcClient, IOptions<TorrentClientSettings> settings, ILogger<TransmissionClient> logger)
        {
            _rpcClient = rpcClient ?? throw new ArgumentNullException(nameof(rpcClient));
            _settings = settings;
            _logger = logger;
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

        public async Task ReAddTorrentAsync(Stream torrentFileStream, int torrentId)
        {
            var existingTorrent = (await _rpcClient.TorrentGetAsync(new string[]
                {
                    TorrentFields.DOWNLOAD_DIR,
                    TorrentFields.FILE_STATS,
                    TorrentFields.STATUS,
                    TorrentFields.BANDWIDTH_PRIORITY,
                    TorrentFields.PEER_LIMIT
                }, torrentId).ConfigureAwait(false)).Torrents.Single();


            torrentFileStream.Seek(0, SeekOrigin.Begin);
            using var ms = new MemoryStream();
            torrentFileStream.CopyTo(ms);
            var metaInfo = Convert.ToBase64String(ms.ToArray());

            //Remove current torrent
            _rpcClient.TorrentRemove(new int[] { torrentId }, deleteData: false);

            var newTorrent = new NewTorrent
            {
                Metainfo = metaInfo,
                DownloadDirectory = existingTorrent.DownloadDir,
                FilesWanted = GetWantedFilesArray(existingTorrent),
                BandwidthPriority = existingTorrent.BandwidthPriority,
                Paused = existingTorrent.Status == 0, //0 means stopped
                PeerLimit = existingTorrent.PeerLimit
            };

            //Add again
            await _rpcClient.TorrentAddAsync(newTorrent).ConfigureAwait(false);
        }

        private static int[] GetWantedFilesArray(TorrentInfo existingTorrent)
        {
            var wantedFiles = new List<int>();
            for (int i = 0; i < existingTorrent.FileStats.Length; i++)
            {
                if (existingTorrent.FileStats[i].Wanted)
                {
                    wantedFiles.Add(i);
                }
            }

            return wantedFiles.ToArray();
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

        public Task<Stream> DownloadTorrentFileAsync(Torrent torrent)
        {
            var updatedTorrentFilePath = Path.Combine(_settings.Value.TorrentFileDirMapping, Path.GetFileName(torrent.TorrentFilePath));
            _logger.LogDebug("Mapped torrent file location from '{0}' to {1}", torrent.TorrentFilePath, updatedTorrentFilePath);
            try
            {
                if(!File.Exists(updatedTorrentFilePath))
                {
                    var fileName = Path.GetFileName(updatedTorrentFilePath);
                    var fileNameParts = fileName.Split('.');
                    if(fileNameParts.Length > 1)
                    {
                        //This could be from transmission changing the way they store torrent files over the past few versions
                        var newPath = updatedTorrentFilePath.Replace(fileName, $"{fileNameParts[fileNameParts.Length - 2]}.{fileNameParts[fileNameParts.Length - 1]}");
                        _logger.LogDebug("Could not find torrent at {0}, might be stale info - trying {1}", updatedTorrentFilePath, newPath);
                        updatedTorrentFilePath = newPath;
                    }
                }

                return Task.FromResult((Stream)File.OpenRead(updatedTorrentFilePath));
            }
            catch (FileNotFoundException e)
            {
                throw new InvalidOperationException($"Could not find torrent file for torrent {torrent.Name}, original location '{torrent.TorrentFilePath}' mapped location '{updatedTorrentFilePath}'.", e);
            }
        }
    }
}