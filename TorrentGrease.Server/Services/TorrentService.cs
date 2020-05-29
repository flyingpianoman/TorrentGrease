using Grpc.Core.Logging;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TorrentGrease.Shared.ServiceContracts;
using TorrentGrease.Shared.ServiceContracts.TorrentRequests;
using TorrentGrease.Shared.TorrentClient;
using TorrentGrease.TorrentClient;

namespace TorrentGrease.Server.Services
{
    public class TorrentService : ITorrentService
    {
        private readonly ITorrentClient _torrentClient;
        private readonly ILogger<TorrentService> _logger;

        public TorrentService(ITorrentClient torrentClient, ILogger<TorrentService> logger)
        {
            _logger = logger;
            _torrentClient = torrentClient ?? throw new ArgumentNullException(nameof(torrentClient));
        }

        public async ValueTask<IEnumerable<Torrent>> GetAllTorrentsAsync()
        {
            return await _torrentClient.GetAllTorrentsAsync().ConfigureAwait(false);
        }


        public async ValueTask MapTorrentsToDiskAsync(MapTorrentsToDiskRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            if (request.PathsToScan == null || request.TorrentIds == null)
            {
                return;
            }

            var torrents = await _torrentClient.GetTorrentsByIDsAsync(request.TorrentIds);
            _logger.LogDebug("Found {0} torrents", torrents.Count());

            var extensionsWhitelist = torrents
                .SelectMany(t => t.Files)
                .Select(tf => Path.GetExtension(tf.FileLocationInTorrent))
                .Distinct()
                .ToArray();

            var filePathsByFileNameLookup = GetFilePathsByFileNameLookup(request.PathsToScan, extensionsWhitelist);
            _logger.LogDebug("Found {0} files to go trough", filePathsByFileNameLookup.Count());

            foreach (var torrent in torrents)
            {
                _logger.LogDebug("Mapping torrent {0}", torrent.Name);
                var largestFileSize = torrent.Files.Max(f => f.SizeInBytes);
                var biggestFileInTorrent = torrent.Files.First(f => f.SizeInBytes == largestFileSize);
                var fileNameToSearch = Path.GetFileName(biggestFileInTorrent.FileLocationInTorrent);

                _logger.LogDebug("Searching for a file named '{0}' with a size of {1}B", fileNameToSearch, largestFileSize);
                var matchingFiles = filePathsByFileNameLookup.Contains(fileNameToSearch)
                    ? filePathsByFileNameLookup[fileNameToSearch].Select(f => new FileInfo(f))
                    : new FileInfo[] { };
                
                _logger.LogDebug("Found {0} matches by filename: {1}", matchingFiles.Count(),
                    string.Join(", ", matchingFiles.Select(fi => $"<{fi.Name}, {fi.Length}B>")));

                matchingFiles = matchingFiles
                    .Where(fi => fi.Length == biggestFileInTorrent.SizeInBytes)
                    .ToArray();

                if (!matchingFiles.Any())
                {
                    _logger.LogDebug("No matching files found by filename and size");
                    continue;
                }

                _logger.LogDebug("Found {0} matches by filename and size: {1}", matchingFiles.Count(),
                    string.Join(", ", matchingFiles.Select(fi => $"<{fi.Name}, {fi.Length}B>")));
            }
        }

        private static ILookup<string, string> GetFilePathsByFileNameLookup(IEnumerable<string> pathsToScan, string[] extensionsWhitelist)
        {
            var filePaths = new List<string>();
            var pattern = string.Join("|", extensionsWhitelist.Select(ext => $"*.{ext}"));

            foreach (var pathToScan in pathsToScan)
            {
                filePaths.AddRange(Directory.GetFiles(pathToScan, pattern, SearchOption.AllDirectories));
            }

            return filePaths
                .ToLookup(f => Path.GetFileName(f), f => f);
        }
    }
}
