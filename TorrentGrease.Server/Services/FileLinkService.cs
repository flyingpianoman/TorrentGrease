using Microsoft.Extensions.Logging;
using Mono.Unix;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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

        public ValueTask CreateFileLinksAsync(IEnumerable<FileLinkToCreate> fileLinksToCreate)
        {
            if (fileLinksToCreate == null || !fileLinksToCreate.Any())
            {
                return ValueTask.CompletedTask;
            }

            if (Environment.OSVersion.Platform != PlatformID.Unix)
            {
                throw new PlatformNotSupportedException("Only unix/linux platforms are supported");
            }

            if(fileLinksToCreate.Any(l => l.FilePaths.Count() < 2))
            {
                throw new InvalidOperationException("One or more file links to create have less than 2 paths defined");
            }

            _logger.LogInformation("Creating file links for {nrOfFileLinks} unique data blobs", fileLinksToCreate.Count());

            foreach (var fileLinkToCreate in fileLinksToCreate)
            {
                _logger.LogInformation("Analyzing the following paths to deduplicate them by replacing them with filelinks: {paths}", string.Join(", ", fileLinkToCreate.FilePaths));

                //For simplicity's sake we're going use the first files inode as the inode we're going to link the rest to
                var fileToBaseLinkOff = UnixFileSystemInfo.GetFileSystemEntry(fileLinkToCreate.FilePaths.First());
                _logger.LogInformation("Basing file links for this candidate off {source}", fileToBaseLinkOff.FullName);

                foreach (var fileToLink in fileLinkToCreate.FilePaths.Skip(1))
                {
                    _logger.LogInformation("Processing file link to create between {source} {target}", fileToBaseLinkOff.FullName, fileToLink);
                    var fileToLinkInfo = UnixFileSystemInfo.GetFileSystemEntry(fileToLink);
                    var shouldSkip = VerifyIfWeShouldSkip(fileToBaseLinkOff, fileToLinkInfo);

                    if (shouldSkip)
                    {
                        continue;
                    }

                    var tmpFileLinkName = Path.Combine(Path.GetDirectoryName(fileToLink) ?? throw new InvalidOperationException(), "torrent-grease-tmp-file-link-" + Guid.NewGuid().ToString("N"));
                    if (tmpFileLinkName.Length > 255)
                    {
                        _logger.LogWarning("Skipping because Tmp file path {tmpFileLinkName} would be larger than max filepath length of many filesystems (255)", tmpFileLinkName);
                        continue;
                    }

                    CreateFileLink(fileToBaseLinkOff, tmpFileLinkName);
                    OverwriteOriginalFileWithTmpFileLink(fileToLink, tmpFileLinkName);
                }
            }

            return ValueTask.CompletedTask;
        }

        private void OverwriteOriginalFileWithTmpFileLink(string fileToLink, string tmpFileLinkName)
        {
            File.Move(tmpFileLinkName, fileToLink, overwrite: true);
            _logger.LogInformation("Overwritten {fileToLink} with tmp link file {tmpFileLinkName}", tmpFileLinkName, fileToLink);
        }

        private void CreateFileLink(UnixFileSystemInfo fileToBaseLinkOff, string tmpFileLinkName)
        {
            if (File.Exists(tmpFileLinkName))
            {
                _logger.LogWarning("{tmpFileLinkName} already exists, deleting it", tmpFileLinkName);
                File.Delete(tmpFileLinkName);
            }

            fileToBaseLinkOff.CreateLink(tmpFileLinkName);
            _logger.LogInformation("Linked {tmpFileLinkName} to the inode of {fileToBaseLinkOff}", tmpFileLinkName, fileToBaseLinkOff.FullName);
        }

        private bool VerifyIfWeShouldSkip(UnixFileSystemInfo firstFileInfo, UnixFileSystemInfo loopingFileInfo)
        {
            if (firstFileInfo.Length != loopingFileInfo.Length)
            {
                _logger.LogInformation("Files to link have different sizes, skipping: {path1} {path2}", firstFileInfo.FullName, loopingFileInfo.FullName);
                return true;
            }

            if (firstFileInfo.Device == loopingFileInfo.Device &&
                firstFileInfo.Inode == loopingFileInfo.Inode)
            {
                _logger.LogInformation("Files to link are already linked, skipping: {path1} {path2}", firstFileInfo.FullName, loopingFileInfo.FullName);
                return true;
            }

            if (firstFileInfo.Device != loopingFileInfo.Device)
            {
                _logger.LogInformation("Files don't share a device id, skipping {path1} {path2}", firstFileInfo.FullName, loopingFileInfo.FullName);
                return true;
            }

            return false;
        }

        public async Task<IEnumerable<FileLinkCandidate>> ScanForFilesToLinkAsync(ScanForPossibleFileLinksRequest request)
        {
            if (request.PathsToScan == null || !request.PathsToScan.Any())
            {
                return Enumerable.Empty<FileLinkCandidate>();
            }

            if (Environment.OSVersion.Platform != PlatformID.Unix)
            {
                throw new PlatformNotSupportedException("Only unix/linux platforms are supported");
            }

            var filesBySizeLookup = CreateFilesBySizeLookupWhereSizeHasAtLeast1Entry(request);
            var fileLinkCandidates = await FileFileLinkCandidatesAysnc(filesBySizeLookup, request.FullByteComparisonMaxFileSize);

            _logger.LogInformation("Found {nrOfPaths} files which can be reduced to {nrOfFileLinks} file links.", fileLinkCandidates.Sum(c => c.FilePaths.Count), fileLinkCandidates.Count());
            return fileLinkCandidates;
        }

        private async Task<IEnumerable<FileLinkCandidate>> FileFileLinkCandidatesAysnc
            (Dictionary<long, List<UnixFileSystemInfo>> filesBySizeLookup, long fullByteComparisonMaxFileSize)
        {
            _logger.LogInformation("Compare files of same size to look for file link candidates, found {nrOfSizes} unique file sizes and {nrOfFiles} files",
                filesBySizeLookup.Count, filesBySizeLookup.Sum(g => g.Value.Count));

            var fileLinkCandidates = new ConcurrentBag<FileLinkCandidate>();
            var nrOfRemainingUniqueSizes = filesBySizeLookup.Count;

            await Parallel.ForEachAsync(filesBySizeLookup.Values, 
                new ParallelOptions { MaxDegreeOfParallelism = 2 },
                async (filesWithSameSize, ct) =>
            {
                var potentialCandidates = await FindPotentialCandidatesForAsync(filesWithSameSize, fullByteComparisonMaxFileSize);
                potentialCandidates = GetCandidatesThatArentFileLinksAlready(potentialCandidates);

                foreach (var potentialCandidate in potentialCandidates)
                {
                    fileLinkCandidates.Add(potentialCandidate);
                }

                _logger.LogInformation("{nrOfRemainingUniqueSizes} unique sizes remain", --nrOfRemainingUniqueSizes);
            });

            return fileLinkCandidates.ToArray();
        }

        private async Task<IEnumerable<FileLinkCandidate>> FindPotentialCandidatesForAsync
            (List<UnixFileSystemInfo> filesWithSameSize, long fullByteComparisonMaxFileSize)
        {
            var skipList = new List<int>();
            var potentialCandidates = new List<FileLinkCandidate>();

            for (int file1Index = 0; file1Index < filesWithSameSize.Count; file1Index++)
            {
                var file1 = filesWithSameSize[file1Index];

                //Skip if this path is already added, or is already a hardlink
                if (skipList.Contains(file1Index))
                {
                    continue;
                }

                //Compare current element with all elements after it
                //this ensures items only get compared once
                for (int file2Index = file1Index + 1; file2Index < filesWithSameSize.Count; file2Index++)
                {
                    if (skipList.Contains(file2Index))
                    {
                        continue;
                    }

                    var file2 = filesWithSameSize[file2Index];

                    //Files are hard linked, or they're similar or they're too big to compare in a reasonable time
                    if (FilesAreLinksToTheSameInode(file1, file2) || 
                        (file1.Length > fullByteComparisonMaxFileSize || await FileContentsAreEqualAsync(file1, file2)))
                    {
                        var fileLinkCandidate = potentialCandidates.FirstOrDefault(c => c.FilePaths.Any(fp => fp.FilePath == file1.FullName));

                        if (fileLinkCandidate == null)
                        {
                            _logger.LogDebug("Added {file} to the the canidates list", file1.FullName);
                            _logger.LogDebug("Added {file} to the the canidates list", file2.FullName);

                            fileLinkCandidate = new FileLinkCandidate
                            {
                                FileSizeInBytes = file1.Length,
                                FilePaths = new List<FileLinkCandidateFile>
                                    {
                                        new FileLinkCandidateFile { FilePath = file1.FullName, DeviceId = file1.Device, InodeId = file1.Inode},
                                        new FileLinkCandidateFile { FilePath = file2.FullName, DeviceId = file2.Device, InodeId = file2.Inode},
                                    }
                            };
                            potentialCandidates.Add(fileLinkCandidate);
                        }
                        else
                        {
                            _logger.LogDebug("Added {file} to the the canidates list", file2.FullName);
                            fileLinkCandidate.FilePaths.Add(new FileLinkCandidateFile { FilePath = file2.FullName, DeviceId = file2.Device, InodeId = file2.Inode });
                        }

                        skipList.Add(file2Index);
                    }
                }
            }

            return potentialCandidates;
        }

        private static bool FilesAreLinksToTheSameInode(UnixFileSystemInfo file1, UnixFileSystemInfo file2)
        {
            return file1.Device == file2.Device 
                && file1.Inode == file2.Inode;
        }

        private IEnumerable<FileLinkCandidate> GetCandidatesThatArentFileLinksAlready
            (IEnumerable<FileLinkCandidate> potentialCandidates)
        {
            return potentialCandidates
                .Where(potentialCandidate =>
                {
                    var deviceId = potentialCandidate.FilePaths[0].DeviceId;
                    var inodeId = potentialCandidate.FilePaths[0].InodeId;
                    var allPathsInCandidateAreFileLinks = potentialCandidate.FilePaths.Skip(1).All(fp => fp.DeviceId == deviceId && fp.InodeId == inodeId);
                    
                    if(allPathsInCandidateAreFileLinks)
                    {
                        _logger.LogDebug("Skpping {filesToSkip} since they already link to the same data", String.Join(", ", potentialCandidate.FilePaths.Select(fp => $"'{fp.FilePath}'")));
                    }

                    return !allPathsInCandidateAreFileLinks;
                })
                .ToArray();
        }

        private Dictionary<long, List<UnixFileSystemInfo>> CreateFilesBySizeLookupWhereSizeHasAtLeast1Entry(
            ScanForPossibleFileLinksRequest request)
        {
            var filesBySizeLookup = new Dictionary<long, List<UnixFileSystemInfo>>();
            var minBytes = request.MinFileSizeInBytes;
            var fileExtExclusionRegex = string.IsNullOrWhiteSpace(request.FileExtensionExclusionRegex)
                ? null
                : new Regex(request.FileExtensionExclusionRegex, RegexOptions.Compiled);

            _logger.LogInformation("Scanning {nrOfPathsToScan} paths for files to analyze for possible file links later", request.PathsToScan.Count());
            foreach (var pathToScan in request.PathsToScan)
            {
                var files = Directory.GetFiles(pathToScan, "*", SearchOption.AllDirectories);
                _logger.LogInformation("Found {nrOfFiles} files on path '{pathToScan}'", files.Length, pathToScan);

                if(fileExtExclusionRegex != null)
                {
                    files = files
                        .Where(f => !fileExtExclusionRegex.IsMatch(Path.GetExtension(f)))
                        .ToArray();
                    _logger.LogInformation("Reduced to {nrOfFiles} files after excluding files based on the extension exclusion regex", pathToScan);
                }

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
                        _logger.LogInformation("Skpping '{file}', it's a symlink which we don't support atm", file);
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
            return filesBySizeLookup;
        }

        private async Task<bool> FileContentsAreEqualAsync(UnixFileSystemInfo file1, UnixFileSystemInfo file2)
        {
            _logger.LogDebug("Comparing {file1} to {file2}", file1.FullName, file2.FullName);
            var sw = Stopwatch.StartNew();
            var logAfterXSeconds = 3;

            try
            {
                const int bufferSize = 1024 * 10;
                var iterations = (int)Math.Ceiling((double)file1.Length / bufferSize);

                using (var fs1 = File.OpenRead(file1.FullName))
                using (var fs2 = File.OpenRead(file2.FullName))
                {
                    var byteBuffer1 = new byte[bufferSize];
                    var byteBuffer2 = new byte[bufferSize];

                    for (int i = 0; i < iterations; i++)
                    {
                        if (sw.Elapsed.TotalSeconds >= logAfterXSeconds)
                        {
                            var progress = decimal.Round((decimal)i / iterations * 100, 2);
                            var elapsedTime = sw.Elapsed;
                            var expectedTotalTime = progress == 0 ? TimeSpan.Zero : TimeSpan.FromMilliseconds(sw.Elapsed.TotalMilliseconds / Convert.ToDouble(progress) * 100);
                            _logger.LogDebug("Comparing {file1} to {file2}, progress: {prcnt}%. Elapsed {elapsedTime}, expected total time {expectedTotalTime}",
                                file1.FullName, file2.FullName, progress, elapsedTime, expectedTotalTime);
                            logAfterXSeconds += 3;
                        }

                        await fs1.ReadAsync(byteBuffer1.AsMemory(0, bufferSize));
                        await fs2.ReadAsync(byteBuffer2.AsMemory(0, bufferSize));

                        // Byte arrays can be converted directly to ReadOnlySpan
                        if (!((ReadOnlySpan<byte>)byteBuffer1).SequenceEqual((ReadOnlySpan<byte>)byteBuffer2))
                        {
                            _logger.LogDebug("{file1} and {file2} are **NOT** equal", file1.FullName, file2.FullName);
                            return false;
                        }
                    }
                }
                
                _logger.LogDebug("{file1} and {file2} are equal", file1.FullName, file2.FullName);
                return true;
            }
            finally
            {
                sw.Stop();
                _logger.LogDebug("Done comparing {file1} to {file2}, took {timeTook}", file1.FullName, file2.FullName, sw.Elapsed);
            }
        }
    }
}
