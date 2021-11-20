using Docker.DotNet;
using FluentAssertions;
using SpecificationTest.Crosscutting;
using SpecificationTest.Pages;
using SpecificationTest.Steps.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using TestUtils;
using TestUtils.Torrent;
using TorrentGrease.TorrentClient;

namespace SpecificationTest.Steps
{
    [Binding]
    public class TorrentOverviewSteps : StepsBase
    {
        private readonly TorrentFileHelper _torrentFileHelper = DI.Get<TorrentFileHelper>();
        private readonly ITorrentClient _torrentClient = DI.Get<ITorrentClient>();

        [Then(@"I see an overview containing 0 torrents")]
        public void ThenISeeAnOverviewOfTheTorrents()
        {
            InnerAsync().GetAwaiter().GetResult();

            async Task InnerAsync()
            {
                var page = WebDriver.CurrentPageAs<TorrentOverviewPage>();
                await page.InitializeAsync().ConfigureAwait(false);
                var torrentComps = await page.GetTorrentComponentsAsync().ConfigureAwait(false);
                torrentComps.Count().Should().Be(0);
            }
        }

        [Given(@"the following torrents are staged")]
        public void GivenTheFollowingTorrentsAreStaged(Table table)
        {
            InnerAsync().GetAwaiter().GetResult();

            async Task InnerAsync()
            {
                var torrentsToStage = table.CreateSet<StageTorrentDto>().ToList();

                foreach (var torrentDto in torrentsToStage)
                {
                    await CreateAndAddTorrentAsync(torrentDto).ConfigureAwait(false);
                }

                await WaitUntilTorrentClientProcessedTheTorrentsAsync(torrentsToStage).ConfigureAwait(false);
            }

        }

        private async Task WaitUntilTorrentClientProcessedTheTorrentsAsync(List<StageTorrentDto> torrentsToStage)
        {
            await Polly
                .Policy
                .Handle<PageHelper.RetryException>()
                .WaitAndRetryUntilTimeoutAsync(TimeSpan.FromMilliseconds(10), TimeSpan.FromSeconds(5))
                .ExecuteAsync(async () =>
                {
                    var torrents = await _torrentClient.GetAllTorrentsAsync().ConfigureAwait(false);
                    if (torrents.Count() != torrentsToStage.Count)
                    {
                        throw new PageHelper.RetryException();
                    }
                }).ConfigureAwait(false);
        }

        private async Task CreateAndAddTorrentAsync(StageTorrentDto torrentDto)
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            var torrentFilePath = Path.Combine(tempDir, torrentDto.Name + ".torrent");
            var torrentDataDir = Path.Combine(tempDir, torrentDto.Name);
            Directory.CreateDirectory(torrentDataDir);

            var torrentFileMappings = await CreateTorrentFilesAsync(torrentDto, torrentDataDir).ConfigureAwait(false);

            var newTorrentFile = new NewTorrentFile
            {
                Name = torrentDto.Name,
                TrackerAnnounceUrls = new string[] { torrentDto.TrackerAnnounceUrl1, torrentDto.TrackerAnnounceUrl2 }
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList(),
                FileMappings = torrentFileMappings
            };

            await _torrentFileHelper.CreateTorrentAsync(torrentFilePath, newTorrentFile);
            await _torrentClient.AddTorrentAsync(torrentDto.Name, torrentFilePath, torrentDto.Location, newTorrentFile.FileMappings.Count()).ConfigureAwait(false);
            DI.Get<Dictionary<string, string>>("TorrentDataFolders")[torrentDto.Name] = torrentDataDir;
        }

        private async Task<List<CreateTorrentFileMapping>> CreateTorrentFilesAsync(
            StageTorrentDto torrentDto, string tempDir)
        {
            var torrentFileMappings = new List<CreateTorrentFileMapping>();
            var torrentFiles = new[]
                {
                    new { FilePath = torrentDto.TorrentFile1Path, SizeInBytes = torrentDto.TorrentFile1SizeInKB * 1024 },
                    new { FilePath = torrentDto.TorrentFile2Path, SizeInBytes = torrentDto.TorrentFile2SizeInKB * 1024 }
                }
                .Where(t => !string.IsNullOrWhiteSpace(t.FilePath))
                .ToArray();

            var torrentFileHelper = DI.Get<TorrentFileHelper>();
            foreach (var torrentFile in torrentFiles)
            {
                var subDirs = Path.GetDirectoryName(torrentFile.FilePath);

                if (!string.IsNullOrWhiteSpace(subDirs))
                {
                    Directory.CreateDirectory(Path.Combine(tempDir, subDirs));
                }

                var torrentFilePath = Path.Combine(tempDir, torrentFile.FilePath);
                await torrentFileHelper.CreateTextFileAsync(torrentFilePath, (int)torrentFile.SizeInBytes);

                torrentFileMappings.Add(new CreateTorrentFileMapping
                {
                    FileLocOnDisk = torrentFilePath,
                    FileLocInTorrent = torrentFile.FilePath.Replace('/', '\\')
                });
            }

            return torrentFileMappings;
        }

        [Then(@"I see an overview of the following torrents")]
        public void ThenISeeAnOverviewOfTheFollowingTorrents(Table table)
        {
            InnerAsync().GetAwaiter().GetResult();

            async Task InnerAsync()
            {
                var propertiesToAssert = table.Header;

                var expectedTorrents = table.CreateSet<TorrentOverviewRowDto>().ToList();
                var page = WebDriver.CurrentPageAs<TorrentOverviewPage>();
                var actualTorrents = await page.GetTorrentComponentsAsync().ConfigureAwait(false);

                AssertTorrents(actualTorrents, expectedTorrents, propertiesToAssert);
            }
        }

        [When(@"I select the following torrents")]
        public void WhenISelectTheFollowingTorrents(Table table)
        {
            InnerAsync().GetAwaiter().GetResult();

            async Task InnerAsync()
            {
                var torrentNames = table.Rows.Select(r => r.Values.First()).ToList();
                var page = WebDriver.CurrentPageAs<TorrentOverviewPage>();
                var torrents = (await page.GetTorrentComponentsAsync().ConfigureAwait(false))
                    .Where(t => torrentNames.Contains(t.Name))
                    .ToArray();

                torrents.Length.Should().Be(torrentNames.Count);

                foreach (var torrent in torrents)
                {
                    torrent.IsSelected = true;
                }
            }
        }

        [When(@"I open the relocate data dialog for the following torrents")]
        public void WhenOpenTheRelocateDataDialog(Table table)
        {
            WhenISelectTheFollowingTorrents(table);

            var page = WebDriver.CurrentPageAs<TorrentOverviewPage>();
            page.ShowRelocateTorrentsModalButton.Click();
        }

        [When(@"I scan for torrent data relocate candidates with the following paths")]
        public void WhenIScanForTorrentDataRelocateCandidatesWithTheFollowingPaths(Table table)
        {
            InnerAsync().GetAwaiter().GetResult();

            async Task InnerAsync()
            {
                WhenISetTheFollowingPathsToScan(table);
                var page = WebDriver.CurrentPageAs<TorrentOverviewPage>();
                var dialog = await page.GetScanForRelocationCandidatesComponentAsync().ConfigureAwait(false);

                dialog.ClickScanForCandidatesButton();
            }
        }


        [When(@"I set the following paths to scan")]
        public void WhenISetTheFollowingPathsToScan(Table table)
        {
            InnerAsync().GetAwaiter().GetResult();

            async Task InnerAsync()
            {
                var pathsToScan = table.Rows.Select(r => r[0]).ToList();
                var page = WebDriver.CurrentPageAs<TorrentOverviewPage>();
                var dialog = await page.GetScanForRelocationCandidatesComponentAsync().ConfigureAwait(false);
                var first = true;

                foreach (var pathToScan in pathsToScan)
                {
                    if (!first)
                    {
                        dialog.ClickAddPathToScanButton();
                    }
                    else
                    {
                        first = false;
                    }

                    dialog.PathToScanElements.Last().SendKeys(pathToScan);
                }
            }
        }

        [Then(@"the following torrents have no relocation options")]
        public void ThenTheFollowingTorrentsHaveNoRelocationOptions(Table table)
        {
            InnerAsync().GetAwaiter().GetResult();

            async Task InnerAsync()
            {
                var torrentNames = table.Rows.Select(r => r[0]).ToList();
                var page = WebDriver.CurrentPageAs<TorrentOverviewPage>();
                var modal = await page.GetPickRelocationCandidatesComponentAsync().ConfigureAwait(false);

                foreach (var torrentName in torrentNames)
                {
                    var candidate = modal.TorrentRelocationCandidates.Single(c => c.TorrentName == torrentName);
                    candidate.RelocateOptionsCount.Should().Be(0);
                }
            }
        }

        private static void AssertTorrents(IEnumerable<Pages.Components.TorrentOverview.TorrentComponent> actualTorrents,
            IEnumerable<TorrentOverviewRowDto> expectedTorrents,
            ICollection<string> propertiesToAssert)
        {
            actualTorrents.Count().Should().Be(expectedTorrents.Count());

            foreach (var expectedTorrent in expectedTorrents)
            {
                var actualTorrent = actualTorrents.Single(p => p.Name == expectedTorrent.Name);
                if (propertiesToAssert.Contains(nameof(expectedTorrent.GBsOnDisk)))
                {
                    actualTorrent.GBsOnDisk.Should().Be(expectedTorrent.GBsOnDisk);
                }

                if (propertiesToAssert.Contains(nameof(expectedTorrent.LocationOnDisk)))
                {
                    actualTorrent.Location.Should().Be(expectedTorrent.LocationOnDisk);
                }

                if (propertiesToAssert.Contains(nameof(expectedTorrent.TotalSizeInGB)))
                {
                    actualTorrent.SizeInGB.Should().Be(expectedTorrent.TotalSizeInGB);
                }

                if (propertiesToAssert.Contains(nameof(expectedTorrent.TotalUploadedInGB)))
                {
                    actualTorrent.TotalUploadInGB.Should().Be(expectedTorrent.TotalUploadedInGB);
                }

                if (propertiesToAssert.Contains(nameof(expectedTorrent.TrackerAnnounceUrl1)))
                {
                    AssertTrackers(expectedTorrent, actualTorrent);
                }

                if (propertiesToAssert.Contains(nameof(expectedTorrent.Error)))
                {
                    actualTorrent.Error.Should().Be(expectedTorrent.Error);
                }
            }
        }

        private static void AssertTrackers(TorrentOverviewRowDto expectedTorrent, Pages.Components.TorrentOverview.TorrentComponent actualTorrent)
        {
            var expectedTrackerUrls = new[] { expectedTorrent.TrackerAnnounceUrl1, expectedTorrent.TrackerAnnounceUrl2 }
                                .Where(s => !string.IsNullOrWhiteSpace(s))
                                .ToArray();

            actualTorrent.TrackerUrls.Count.Should().Be(expectedTrackerUrls.Length);

            foreach (var expectedTrackerUrl in expectedTrackerUrls)
            {
                actualTorrent.TrackerUrls.Contains(expectedTrackerUrl).Should().BeTrue();
            }
        }

        [Given(@"the data of the following torrents is sent to the torrent client")]
        public void GivenTheDataOfTheFollowingTorrentsIsSentToTheTorrentClient(Table table)
        {
            InnerAsync().GetAwaiter().GetResult();

            async Task InnerAsync()
            {
                var copyTorrentDataRequests = table.CreateSet<CopyTorrentDataDto>().ToList();
                var dockerClient = DI.Get<DockerClient>();
                var transmissionContainerId = await dockerClient.Containers.GetContainerIdByNameAsync(TestSettings.TransmissionContainerName).ConfigureAwait(false);
                var torrentDataDirs = DI.Get<Dictionary<string, string>>("TorrentDataFolders");
                var torrentsIDsToVerify = new List<int>();

                foreach (var copyTorrentDataRequest in copyTorrentDataRequests)
                {
                    var dataDirPath = torrentDataDirs[copyTorrentDataRequest.TorrentName];
                    var tarStream = ArchiveHelper.CreateDirectoryTarStream(dataDirPath);

                    await dockerClient.CreateDirectoryStructureInContainerAsync(transmissionContainerId, copyTorrentDataRequest.TargetLocation).ConfigureAwait(false);
                    await dockerClient.UploadTarredFileToContainerAsync(tarStream, transmissionContainerId, copyTorrentDataRequest.TargetLocation).ConfigureAwait(false);
                    await WaitUntilFilesExistInContainerAsync(dockerClient, transmissionContainerId, copyTorrentDataRequest, dataDirPath);

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

        private static async Task WaitUntilFilesExistInContainerAsync(DockerClient dockerClient, string transmissionContainerId, CopyTorrentDataDto copyTorrentDataRequest, string dataDirPath)
        {
            var basePath = Directory.GetParent(dataDirPath).FullName;
            foreach (var filePath in Directory.GetFiles(dataDirPath, "*", SearchOption.AllDirectories))
            {
                var filePathInContainer = Path.Combine(copyTorrentDataRequest.TargetLocation + "/" + Path.GetRelativePath(basePath, filePath).Replace('\\', '/'));
                Thread.Sleep(1000);
                await Polly
                    .Policy
                    .Handle<PageHelper.RetryException>()
                    .WaitAndRetryUntilTimeoutAsync(TimeSpan.FromMilliseconds(10), TimeSpan.FromSeconds(30))
                    .ExecuteAsync(async () =>
                    {
                        var fileExists = await dockerClient.DoesFileSystemObjectExistAsync(transmissionContainerId, filePathInContainer);
                        if (!fileExists)
                        {
                            throw new PageHelper.RetryException();
                        }
                    });
            }
        }

        [Given(@"the following torrents are \(re\)verified by the torrent client")]
        public void GivenTheFollowingTorrentsAreReVerifiedByTheTorrentClient(Table table)
        {
            InnerAsync().GetAwaiter().GetResult();

            async Task InnerAsync()
            {
                var torrentsBeforeVerify = (await _torrentClient.GetAllTorrentsAsync())
                    .Where(t => table.Rows.Any(row => row["TorrentName"] == t.Name))
                    .ToArray();

                torrentsBeforeVerify.Length.Should().Be(table.RowCount);

                foreach (var torrent in torrentsBeforeVerify)
                {
                    await _torrentClient.VerifyTorrentsAsync(new int[] { torrent.ID });
                }

                await WaitForTorrentsToHaveChangedAsync(torrentsBeforeVerify).ConfigureAwait(false);
            }
        }

        private async Task WaitForTorrentsToHaveChangedAsync(TorrentGrease.Shared.TorrentClient.Torrent[] torrentsBeforeVerify)
        {
            var torrentsIDs = torrentsBeforeVerify.Select(t => t.ID).ToArray();
            await Polly
                .Policy
                .Handle<PageHelper.RetryException>()
                .WaitAndRetryUntilTimeoutAsync(TimeSpan.FromMilliseconds(10), TimeSpan.FromSeconds(5))
                .ExecuteAsync(async () =>
                {
                    var currentTorrents = await _torrentClient.GetTorrentsByIDsAsync(torrentsIDs);

                    foreach (var oldTorrent in torrentsBeforeVerify)
                    {
                        var currentTorrent = currentTorrents.First(t => t.ID == oldTorrent.ID);
                        if (currentTorrent.BytesOnDisk == oldTorrent.BytesOnDisk &&
                            currentTorrent.Error == oldTorrent.Error)
                        {
                            throw new PageHelper.RetryException();
                        }
                    }
                }).ConfigureAwait(false);
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

        [When(@"I relocate the data of the following torrents and verify them afterwards")]
        public void WhenIRelocateTheFollowingCandidates(Table table)
        {
            InnerAsync().GetAwaiter().GetResult();

            async Task InnerAsync()
            {
                var relocateTorrentDataDtos = table.CreateSet<RelocateTorrentDataDto>().ToList();
                var page = WebDriver.CurrentPageAs<TorrentOverviewPage>();
                var modal = await page.GetPickRelocationCandidatesComponentAsync().ConfigureAwait(false);

                foreach (var candidate in modal.TorrentRelocationCandidates)
                {
                    var matchingDto = relocateTorrentDataDtos.SingleOrDefault(dto => dto.TorrentName == candidate.TorrentName);

                    if (matchingDto == null)
                    {
                        if (candidate.IsSelectable)
                        {
                            candidate.IsSelected = false;
                        }
                    }
                    else
                    {
                        candidate.IsSelected = true;
                    }
                }

                modal.TorrentRelocationCandidates
                    .Where(c => c.IsSelected)
                    .Count().Should().Be(relocateTorrentDataDtos.Count);

                modal.IsVerifyTorrentsEnabled = true;

                modal.RelocateCandidatesButton.Click();
                modal.WaitUntilClosed();

                page.RefreshButton.Click();
            }
        }

        [Given(@"I see the following error filters")]
        public void GivenISeeTheFollowingErrorFilters(Table table)
        {
            InnerAsync().GetAwaiter().GetResult();

            async Task InnerAsync()
            {
                var page = WebDriver.CurrentPageAs<TorrentOverviewPage>();
                var filtersPanel = await page.EnsureFilterAccordionIsCollapsedAsync().ConfigureAwait(false);

                //Assert
                var errorFilters = filtersPanel.GetErrorFilters();
                errorFilters.Count().Should().Be(table.Rows.Count);

                foreach (var expectedError in table.Rows.Select(r => r["Error filter"]))
                {
                    errorFilters.Any(c => c.Label == expectedError).Should().BeTrue();
                }
            }
        }

        [When(@"I toggle the following error filter")]
        public void WhenIToggleTheFollowingErrorFilter(Table table)
        {
            InnerAsync().GetAwaiter().GetResult();

            async Task InnerAsync()
            {
                var page = WebDriver.CurrentPageAs<TorrentOverviewPage>();
                var filtersPanel = await page.EnsureFilterAccordionIsCollapsedAsync().ConfigureAwait(false);

                //Assert
                var errorFilters = filtersPanel.GetErrorFilters();
                errorFilters.Count().Should().Be(table.Rows.Count);

                foreach (var expectedError in table.Rows.Select(r => r["Error filter"]))
                {
                    var errorFilter = errorFilters.First(c => c.Label == expectedError);
                    errorFilter.Toggle();
                }
            }
        }

        [When(@"I readd the selected torrents")]
        public void WhenIReaddTheSelectedTorrents()
        {
            var page = WebDriver.CurrentPageAs<TorrentOverviewPage>();
            page.ReAddButton.Click();
            page.WaitUntilNotInWaitMode();
        }
    }
}
