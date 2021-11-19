using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TorrentGrease.Shared.ServiceContracts;
using TorrentGrease.Shared.ServiceContracts.FileManagement;
using TorrentGrease.TorrentClient;

namespace TorrentGrease.Server.Services
{
    public class FileManagementService : IFileManagementService
    {
        private readonly ITorrentClient _torrentClient;
        private readonly ILogger<FileManagementService> _logger;

        public FileManagementService(ITorrentClient torrentClient, ILogger<FileManagementService> logger)
        {
            _logger = logger;
            _torrentClient = torrentClient ?? throw new ArgumentNullException(nameof(torrentClient));
        }

        public async Task<IEnumerable<FileRemovalCandidate>> ScanForFilesToRemoveAsync(ScanForFilesToRemoveRequest request)
        {
            var filesWithoutTorrents = new List<FileRemovalCandidate>();
            var filesThatHaveTorrents = await GetFilesThatHaveTorrents().ConfigureAwait(false);

            var minBytes = request.MinFileSizeInBytes;

            foreach (var dirToScan in request.CompletedTorrentPathsToScan ?? new List<MappedDirectory>())
            {
                ScanDirForFilesToRemoveAsync(dirToScan, filesWithoutTorrents, filesThatHaveTorrents, minBytes);
            }

            return filesWithoutTorrents;
        }

        private void ScanDirForFilesToRemoveAsync(MappedDirectory dirToScan, List<FileRemovalCandidate> filesWithoutTorrents, 
            HashSet<string> filesThatHaveTorrents, long minBytes)
        {
            _logger.LogDebug($"Scanning for {dirToScan.TorrentGreaseDir}");

            if (!Directory.Exists(dirToScan.TorrentGreaseDir))
            {
                throw new ArgumentException($"Directory '{dirToScan.TorrentGreaseDir}' was not found.");
            }

            var torrentClientDir = dirToScan.TorrentClientDir;

            if (!torrentClientDir.EndsWith(Path.DirectorySeparatorChar) &&
                !torrentClientDir.EndsWith(Path.AltDirectorySeparatorChar))
            {
                torrentClientDir += Path.DirectorySeparatorChar;
            }

            var torrentGreaseDir = dirToScan.TorrentGreaseDir;

            if (!torrentGreaseDir.EndsWith(Path.DirectorySeparatorChar) &&
                !torrentGreaseDir.EndsWith(Path.AltDirectorySeparatorChar))
            {
                torrentGreaseDir += Path.DirectorySeparatorChar;
            }

            var files = Directory.GetFiles(torrentGreaseDir, "*", SearchOption.AllDirectories);
            _logger.LogDebug($"Found {files.Length} files");

            foreach (var filePath in files)
            {
                AddCandidateIfFitForRemoval(filesWithoutTorrents, filesThatHaveTorrents, minBytes, torrentClientDir, torrentGreaseDir, filePath);
            }
        }

        private static void AddCandidateIfFitForRemoval(List<FileRemovalCandidate> filesWithoutTorrents, HashSet<string> filesThatHaveTorrents, long minBytes, string torrentClientDir, string torrentGreaseDir, string filePath)
        {
            var fileLengthInBytes = new FileInfo(filePath).Length;
            if (fileLengthInBytes < minBytes)
            {
                return;
            }

            var mappedFilePath = Path.Combine(torrentClientDir, filePath[torrentGreaseDir.Length..]);
            if (!filesThatHaveTorrents.Contains(mappedFilePath))
            {
                filesWithoutTorrents.Add(new FileRemovalCandidate
                {
                    FilePath = filePath,
                    FileSizeInBytes = fileLengthInBytes
                });
            }
        }

        private async Task<HashSet<string>> GetFilesThatHaveTorrents()
        {
            var allTorrents = await _torrentClient.GetAllTorrentsAsync().ConfigureAwait(false);
            var filesThatHaveTorrents = new HashSet<string>();

            foreach (var torrent in allTorrents)
            {
                foreach (var torrentFile in torrent.Files)
                {
                    var torrentFilePath = Path.Combine(torrent.Location, torrentFile.FileLocationInTorrent);
                    if (!filesThatHaveTorrents.Contains(torrentFilePath))
                    {
                        filesThatHaveTorrents.Add(torrentFilePath);
                    }
                }

            }

            return filesThatHaveTorrents;
        }

        public ValueTask RemoveFilesAsync(IEnumerable<string> filesToRemove)
        {
            foreach (var fileToRemove in filesToRemove)
            {
                File.Delete(fileToRemove);
            }

            return ValueTask.CompletedTask;
        }
    }
}
