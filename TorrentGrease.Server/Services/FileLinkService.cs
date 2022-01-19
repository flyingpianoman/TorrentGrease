using Microsoft.Extensions.Logging;
using Mono.Unix;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TorrentGrease.Shared.ServiceContracts;
using TorrentGrease.Shared.ServiceContracts.FileLink;

namespace TorrentGrease.Server.Services
{
    public class FileLinkService : IFileLinkService
    {
        private readonly ILogger<FileLinkService> _logger;

        public FileLinkService(ILogger<FileLinkService> logger)
        {
            _logger = logger;
        }

        public ValueTask CreateFileLinksAsync(IEnumerable<FileLinkCandidate> fileLinksToCreate)
        {
            throw new System.NotImplementedException();
            //var a = UnixFileSystemInfo.GetFileSystemEntry("/first");
            //a.CreateLink("/second");
        }

        public Task<IEnumerable<FileLinkCandidate>> ScanForFilesToLinkAsync(ScanForPossibleFileLinksRequest request)
        {
            if (request.PathsToScan == null || !request.PathsToScan.Any())
            {
                return Task.FromResult(Enumerable.Empty<FileLinkCandidate>());
            }

            if (Environment.OSVersion.Platform != PlatformID.Unix)
            {
                throw new PlatformNotSupportedException("Only unix/linux platforms are supported");
            }

            var filesBySizeLookup = new Dictionary<long, List<UnixFileSystemInfo>>();
            var minBytes = request.MinFileSizeInBytes;

            _logger.LogInformation("Scanning {nrOfPathsToScan} paths for files to analyze for possible file links later", request.PathsToScan.Count());
            foreach (var pathToScan in request.PathsToScan)
            {
                var files = Directory.GetFiles(pathToScan, "*", SearchOption.AllDirectories);
                _logger.LogInformation("Found {nrOfFiles} files on path '{pathToScan}'", files.Length, pathToScan);

                foreach (var file in files)
                {
                    var unixFileInfo = UnixFileSystemInfo.GetFileSystemEntry(file);
                    var fileSizeInBytes = unixFileInfo.Length;
                    if (fileSizeInBytes < minBytes)
                    {
                        _logger.LogTrace("Skpping '{file}', it doesn't meet the size criteria", file);
                        continue;
                    }
                    if (UnixFileSystemInfo.GetFileSystemEntry(file).IsSymbolicLink)
                    {
                        _logger.LogTrace("Skpping '{file}', it's a symlink which we don't support atm", file);
                        continue;
                    }

                    if (filesBySizeLookup.ContainsKey(fileSizeInBytes))
                    {
                        filesBySizeLookup[fileSizeInBytes].Add(unixFileInfo);
                    }
                    else
                    {
                        filesBySizeLookup.Add(fileSizeInBytes, new List<UnixFileSystemInfo> { unixFileInfo });
                    }

                    _logger.LogDebug("Current inventory contains {nrOfSizes} unique file sizes and {totalNrOfFiles} total file paths", filesBySizeLookup.Count, filesBySizeLookup.Sum(x => x.Value.Count));
                }
                _logger.LogInformation("Done scanning '{pathToScan}' for files", pathToScan);
            }

            filesBySizeLookup = filesBySizeLookup.Where(kv => kv.Value.Count > 1).ToDictionary(kv => kv.Key, kv => kv.Value);
            _logger.LogInformation("Removed file sizes that had only 1 entry, {nrOfSizes} unique file sizes and {totalNrOfFiles} file paths left", filesBySizeLookup.Count, filesBySizeLookup.Sum(x => x.Value.Count));

            _logger.LogInformation($"Compare files of same size to look for file link candidates");
            var fileLinkCandidates = new ConcurrentBag<FileLinkCandidate>();

            Parallel.ForEach(filesBySizeLookup.Values, filesWithSameSize =>
            {
                var skipList = new List<int>();

                for (int i = 0; i < filesWithSameSize.Count; i++)
                {
                    var file1 = filesWithSameSize[i];

                    //Skip if this path is already added, or is already a hardlink
                    if (skipList.Contains(i))
                    {
                        continue;
                    }

                    //Compare current element with all elements after it
                    //this ensures items only get compared once
                    for (int j = i + 1; j < filesWithSameSize.Count; j++)
                    {
                        if (skipList.Contains(j))
                        {
                            continue;
                        }

                        var file2 = filesWithSameSize[j];

                        //Check if the two files are already hardlink to the same inode
                        if (file1.Device == file2.Device &&
                           file1.Inode == file2.Inode)
                        {
                            _logger.LogTrace("Skpping '{fileToSkip}', it's already sharing the inode of '{fileItSharesWith}'", file2.FullName, file1.FullName);
                            skipList.Add(j);
                            continue;
                        }

                        //TODO replace this with Parallel.ForEachAsync after updating to .net 6 and use await
                        if (FileContentsAreEqualAsync(file1, file2).GetAwaiter().GetResult())
                        {
                            var fileLinkCandidate = fileLinkCandidates.FirstOrDefault(c => c.FilePaths.Contains(file1.FullName));

                            if (fileLinkCandidate == null)
                            {
                                fileLinkCandidate = new FileLinkCandidate
                                {
                                    FileSizeInBytes = file1.Length,
                                    FilePaths = new List<string> { file1.FullName, file2.FullName }
                                };
                                fileLinkCandidates.Add(fileLinkCandidate);
                            }
                            else
                            {
                                fileLinkCandidate.FilePaths.Add(file2.FullName);
                            }

                            skipList.Add(j);
                        }
                    }
                }
            });

            _logger.LogInformation("Found {nrOfPaths} files which can be reduced to {nrOfFileLinks} file links.", fileLinkCandidates.Sum(c => c.FilePaths.Count), fileLinkCandidates.Count);
            return Task.FromResult((IEnumerable<FileLinkCandidate>)fileLinkCandidates);
        }


        static async Task<bool> FileContentsAreEqualAsync(UnixFileSystemInfo file1, UnixFileSystemInfo file2)
        {
            const int bufferSize = sizeof(long); //8 //TODO might try a multiple of 8 to try perf when using less IO trips
            var iterations = (int)Math.Ceiling((double)file1.Length / bufferSize);

            using (var fs1 = File.OpenRead(file1.FullName))
            using (var fs2 = File.OpenRead(file2.FullName))
            {
                var byteBuffer1 = new byte[bufferSize];
                var byteBuffer2 = new byte[bufferSize];
                var memBuffer1 = byteBuffer1.AsMemory();
                var memBuffer2 = byteBuffer2.AsMemory();

                for (int i = 0; i < iterations; i++)
                {
                    await fs1.ReadAsync(memBuffer1).ConfigureAwait(false);
                    await fs2.ReadAsync(memBuffer2).ConfigureAwait(false);

                    if (BitConverter.ToInt64(byteBuffer1, 0) != BitConverter.ToInt64(byteBuffer2, 0))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
