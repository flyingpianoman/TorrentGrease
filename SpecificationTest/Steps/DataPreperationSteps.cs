using Docker.DotNet;
using Microsoft.EntityFrameworkCore;
using SpecificationTest.Crosscutting;
using SpecificationTest.Pages;
using SpecificationTest.Steps.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using TestUtils;
using TestUtils.Torrent;
using TorrentGrease.Data;
using TorrentGrease.Shared;
using TorrentGrease.TorrentClient;

namespace SpecificationTest.Steps
{
    [Binding]
    public class DataPreperationSteps : StepsBase
    {
        private readonly DockerClient _dockerClient = DI.Get<DockerClient>();

        private readonly ITorrentClient _torrentClient = DI.Get<ITorrentClient>();
        private TorrentGreaseDBService TorrentGreaseDBService => DI.Get<TorrentGreaseDBService>();
        private TorrentGreaseDbContext DbContext => TorrentGreaseDBService.DbContext;

        [Given(@"the following trackers are staged")]
        public async Task GivenTheFollowingTrackersAreCreated(Table table)
        {
            foreach (var trackerDto in table.CreateSet<TrackerDto>())
            {
                DbContext.Trackers.Add(new Tracker
                {
                    Name = trackerDto.Name,
                    //TrackerUrls = String.IsNullOrWhiteSpace(trackerDto.TrackerUrl2)
                    //    ? new List<string> { trackerDto.TrackerUrl1 }
                    //    : new List<string> { trackerDto.TrackerUrl1, trackerDto.TrackerUrl2 }
                });
            }

            await DbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        [Given(@"the following policies are staged")]
        public async Task GivenTheFollowingPoliciesAreStaged(Table table)
        {
            foreach (var policyDto in table.CreateSet<PolicyDto>())
            {
                DbContext.Policies.Add(new Policy
                {
                    Name = policyDto.Name,
                    Description = policyDto.Description ?? $"{policyDto.Name} description"
                });
            }

            await DbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        [Given(@"the following tracker policies are staged")]
        public async Task GivenTheFollowingTrackerPoliciesAreStaged(Table table)
        {
            foreach (var trackerPolicyDto in table.CreateSet<TrackerPolicyDto>())
            {
                var policy = DbContext.Policies
                    .Include(p => p.TrackerPolicies)
                    .Single(p => p.Name == trackerPolicyDto.Policy);
                var tracker = DbContext.Trackers.Single(p => p.Name == trackerPolicyDto.Tracker);

                policy.TrackerPolicies.Add(new TrackerPolicy
                {
                    Tracker = tracker
                });
            }

            await DbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        [Given(@"the staged data is uploaded to torrent grease")]
        public async ValueTask GivenTheStagedDataIsUploadedToTorrentGrease()
        {
            await TorrentGreaseDBService.UploadDBContextToContainerAsync().ConfigureAwait(false);
        }

        [Given(@"the following torrent data files are moved")]
        public void GivenTheFollowingTorrentDataFilesAreMoved(Table table)
        {
            InnerAsync().GetAwaiter().GetResult();

            async Task InnerAsync()
            {
                var transmissionContainerId = await _dockerClient.Containers.GetContainerIdByNameAsync(TestSettings.TransmissionContainerName).ConfigureAwait(false);

                foreach (var row in table.Rows)
                {
                    await _dockerClient.MoveFileInContainerAsync(transmissionContainerId, row["From"], row["To"]);
                }
            }
        }

        [Given(@"the data of the following torrents is sent to the torrent client")]
        public void GivenTheDataOfTheFollowingTorrentsIsSentToTheTorrentClient(Table table)
        {
            InnerAsync().GetAwaiter().GetResult();

            async Task InnerAsync()
            {
                var copyTorrentDataRequests = table.CreateSet<CopyTorrentDataDto>().ToList();
                var transmissionContainerId = await _dockerClient.Containers.GetContainerIdByNameAsync(TestSettings.TransmissionContainerName).ConfigureAwait(false);
                var torrentDataDirs = DI.Get<Dictionary<string, string>>("TorrentDataFolders");
                var torrentsIDsToVerify = new List<int>();

                foreach (var copyTorrentDataRequest in copyTorrentDataRequests)
                {
                    var hostDataDirPath = torrentDataDirs[copyTorrentDataRequest.TorrentName];
                    await SendFilesInDataDirToTorrentClientFSAsync(_dockerClient, transmissionContainerId, 
                        copyTorrentDataRequest.TargetLocation, hostDataDirPath).ConfigureAwait(false);

                    if (copyTorrentDataRequest.VerifyTorrent)
                    {
                        var torrent = (await _torrentClient.GetAllTorrentsAsync())
                            .Single(t => t.Name == copyTorrentDataRequest.TorrentName);
                        await _torrentClient.VerifyTorrentsAsync(new int[] { torrent.ID });
                        torrentsIDsToVerify.Add(torrent.ID);
                    }
                }

                if (torrentsIDsToVerify.Any())
                {
                    await WaitForTorrentsToBeMarkedAsDownloadedAsync(torrentsIDsToVerify).ConfigureAwait(false);
                }
            }
        }

        private async Task SendFilesInDataDirToTorrentClientFSAsync(DockerClient dockerClient, string transmissionContainerId,
            string targetLocation, string hostDataDir)
        {
            var tarStream = ArchiveHelper.CreateDirectoryTarStream(hostDataDir);

            await dockerClient.CreateDirectoryStructureInContainerAsync(transmissionContainerId, targetLocation).ConfigureAwait(false);
            await dockerClient.UploadTarredFileToContainerAsync(tarStream, transmissionContainerId, targetLocation).ConfigureAwait(false);

            await WaitUntilFilesInDirExistInContainerAsync(transmissionContainerId, targetLocation, hostDataDir);
        }

        private async Task WaitForTorrentsToBeMarkedAsDownloadedAsync(List<int> torrentsIDsToVerify)
        {
            await Polly
                .Policy
                .Handle<PageHelper.RetryException>()
                .WaitAndRetryUntilTimeoutAsync(TimeSpan.FromMilliseconds(10), TimeSpan.FromSeconds(5))
                .ExecuteAsync(async () =>
                {
                    var torrents = await _torrentClient.GetTorrentsByIDsAsync(torrentsIDsToVerify);
                    if (torrents.Any(t => t.SizeInBytes != t.BytesOnDisk))
                    {
                        throw new PageHelper.RetryException();
                    }
                }).ConfigureAwait(false);
        }

        private async Task WaitUntilFilesInDirExistInContainerAsync(string containerId, 
            string targetLocation, string hostDataDir)
        {
            var basePath = Directory.GetParent(hostDataDir).FullName;
            foreach (var filePath in Directory.GetFiles(hostDataDir, "*", SearchOption.AllDirectories))
            {
                var filePathInContainer = Path.Combine(targetLocation + "/" + Path.GetRelativePath(basePath, filePath).Replace('\\', '/'));
                await WaitUntilFileExistsInContainerAsync(containerId, filePathInContainer);
            }
        }
        [Given(@"the following data is sent to the torrent client")]
        public void GivenTheFollowingDataIsSentToTheTorrentClient(Table table)
        {
            var torrentFileHelper = DI.Get<TorrentFileHelper>();
            InnerAsync().GetAwaiter().GetResult();

            async Task InnerAsync()
            {
                var fileDtos = table.CreateSet<TorrentClientFileDto>().ToList();
                var torrentClientContainerId = await _dockerClient.Containers.GetContainerIdByNameAsync(TestSettings.TransmissionContainerName).ConfigureAwait(false);

                foreach (var fileDto in fileDtos)
                {
                    var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
                    Directory.CreateDirectory(tempDir);
                    var filename = Path.GetFileName(fileDto.FilePath);
                    var fileOnHost = Path.Combine(tempDir, filename);
                    await torrentFileHelper.CreateTextFileAsync(fileOnHost, (int)fileDto.FileSizeInKB * 1024, fileDto.Char ?? '*');

                    var tarStream = ArchiveHelper.CreateSingleFileTarStream(fileOnHost, filename);

                    var fileDir = fileDto.FilePath[..^filename.Length];
                    await _dockerClient.CreateDirectoryStructureInContainerAsync(torrentClientContainerId, fileDir).ConfigureAwait(false);
                    await _dockerClient.UploadTarredFileToContainerAsync(tarStream, torrentClientContainerId, fileDir).ConfigureAwait(false);

                    await WaitUntilFileExistsInContainerAsync(torrentClientContainerId, fileDto.FilePath);
                }
            }
        }

        private async Task WaitUntilFileExistsInContainerAsync(string containerId, string filePath)
        {
            await Polly
                .Policy
                .Handle<PageHelper.RetryException>()
                .WaitAndRetryUntilTimeoutAsync(TimeSpan.FromMilliseconds(10), TimeSpan.FromSeconds(30))
                .ExecuteAsync(async () =>
                {
                    var fileExists = await _dockerClient.DoesFileSystemObjectExistAsync(containerId, filePath);
                    if (!fileExists)
                    {
                        throw new PageHelper.RetryException();
                    }
                });
        }
    }
}
