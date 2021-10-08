using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TorrentGrease.Shared.RelocateTorrent;
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

        public async Task<List<RelocatableTorrentCandidate>> FindRelocatableTorrentCandidatesAsync(MapTorrentsToDiskRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var relocatableTorrentCandidates = new List<RelocatableTorrentCandidate>();

            if (request.PathsToScan == null || request.TorrentIds == null)
            {
                return relocatableTorrentCandidates;
            }

            var torrents = await _torrentClient.GetTorrentsByIDsAsync(request.TorrentIds);
            _logger.LogDebug("Found {0} torrents", torrents.Count());

            var extensionsWhitelist = torrents
                .SelectMany(t => t.Files)
                .Select(tf => Path.GetExtension(tf.FileLocationInTorrent))
                .Distinct()
                .ToArray();

            var filePathsByFileNameLookup = GetFilePathsByFileNameLookup(request.PathsToScan, extensionsWhitelist);
            _logger.LogDebug("Found {0} files to go through", filePathsByFileNameLookup.Count);

            foreach (var torrent in torrents)
            {
                _logger.LogDebug("Looking for data matching torrent {0}", torrent.Name);
                var largestFileSize = torrent.Files.Max(f => f.SizeInBytes);
                var biggestFileInTorrent = torrent.Files.First(f => f.SizeInBytes == largestFileSize);
                var fileNameToSearch = Path.GetFileName(biggestFileInTorrent.FileLocationInTorrent);

                _logger.LogDebug("Searching for a file named '{0}' with a size around {1}B", fileNameToSearch, largestFileSize);
                var matchingFiles = filePathsByFileNameLookup.Contains(fileNameToSearch)
                    ? filePathsByFileNameLookup[fileNameToSearch].Select(f => new FileInfo(f))
                    : Array.Empty<FileInfo>();

                _logger.LogDebug("Found {0} matches by filename: {1}", matchingFiles.Count(),
                    string.Join(", ", matchingFiles.Select(fi => $"<{fi.Name}, {fi.Length}B>")));

                matchingFiles = matchingFiles
                    .Where(fi => fi.Length == biggestFileInTorrent.SizeInBytes)
                    .ToArray();

                if (!matchingFiles.Any())
                {
                    _logger.LogDebug("No matching files found by filename and size");
                }
                else
                {
                    _logger.LogDebug("Found {0} matches by filename and size: {1}", matchingFiles.Count(),
                        string.Join(", ", matchingFiles.Select(fi => $"<{fi.Name}, {fi.Length}B>")));
                }

                var candidates = CreateTorrentRelocateCandidates(biggestFileInTorrent, matchingFiles);
                candidates = GetCandidatesThatMatchAllTorrentFiles(torrent, candidates);

                relocatableTorrentCandidates.Add(new RelocatableTorrentCandidate
                {
                    TorrentID = torrent.ID,
                    TorrentName = torrent.Name,
                    TorrentFilePaths = torrent.Files.Select(t => t.FileLocationInTorrent).ToList(),
                    RelocateOptions = candidates,
                    ChosenOption = candidates.FirstOrDefault()
                });
            }

            return relocatableTorrentCandidates;
        }

        private string[] GetCandidatesThatMatchAllTorrentFiles(Torrent torrent, string[] candidates)
        {
            _logger.LogDebug("Checking matches wether they contain all files that are in the torrent");

            candidates = candidates
                .Where(candidate =>
                {
                    foreach (var torrentFile in torrent.Files)
                    {
                        var torrentFilePathForCandidate = Path.Combine(candidate, torrentFile.FileLocationInTorrent);
                        if (!File.Exists(torrentFilePathForCandidate))
                        {
                            _logger.LogDebug("Removing candidate {0} because {1} was not found", candidate, torrentFile.FileLocationInTorrent);
                            return false;
                        }
                        if (new FileInfo(torrentFilePathForCandidate).Length != torrentFile.SizeInBytes)
                        {
                            _logger.LogDebug("Removing candidate {0} because {1} has a different filesize", candidate, torrentFile.FileLocationInTorrent);
                            return false;
                        }
                    }

                    return true;
                })
                .ToArray();
            return candidates;
        }

        private static string[] CreateTorrentRelocateCandidates(TorrentFile biggestFileInTorrent, IEnumerable<FileInfo> matchingFiles)
        {
            var torrentFileDepth = biggestFileInTorrent.FileLocationInTorrent.Split('/', '\\', StringSplitOptions.RemoveEmptyEntries).Length - 1;
            return matchingFiles.Select(f =>
            {
                var torrentPathCandidate = Path.GetDirectoryName(f.FullName) ?? throw new InvalidDataException();

                for (int i = 0; i < torrentFileDepth; i++)
                {
                    torrentPathCandidate = Directory.GetParent(torrentPathCandidate)?.FullName ?? throw new InvalidDataException();
                }

                return torrentPathCandidate ?? throw new InvalidDataException();
            }).ToArray();
        }


        private static ILookup<string, string> GetFilePathsByFileNameLookup(IEnumerable<string> pathsToScan, string[] extensionsWhitelist)
        {
            var filePaths = new List<string>();

            foreach (var pathToScan in pathsToScan)
            {
                filePaths.AddRange(Directory.EnumerateFiles(pathToScan, "*.*", SearchOption.AllDirectories)
                    .Where(s => extensionsWhitelist.Any(e => s.EndsWith(e, StringComparison.OrdinalIgnoreCase))));
            }

            return filePaths
                .ToLookup(f => Path.GetFileName(f), f => f);
        }

        public async Task RelocateTorrentsAsync(RelocateTorrentsRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if(request.RelocateTorrentCommands == null || !request.RelocateTorrentCommands.Any())
            {
                return;
            }

            foreach (var relocateTorrentCommand in request.RelocateTorrentCommands)
            {
                _logger.LogDebug("Relocating torrent with ID {0} to '{1}'", relocateTorrentCommand.TorrentID, relocateTorrentCommand.NewLocation);
                await _torrentClient.RelocateTorrentAsync(relocateTorrentCommand.TorrentID, relocateTorrentCommand.NewLocation).ConfigureAwait(false);
                _logger.LogDebug("Torrent {0} is relocated", relocateTorrentCommand.TorrentID);
            }

            if(request.VerifyAfterMoving)
            {
                await _torrentClient.VerifyTorrentsAsync(request.RelocateTorrentCommands.Select(c => c.TorrentID).ToArray());
            }
        }
    }
}
