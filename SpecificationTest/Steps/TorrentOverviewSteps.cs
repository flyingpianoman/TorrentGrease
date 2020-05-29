using FluentAssertions;
using MonoTorrent;
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
using TestUtils.Torrent;
using TorrentGrease.TorrentClient;

namespace SpecificationTest.Steps
{
    [Binding]
    public class TorrentOverviewSteps : StepsBase
    {
        private readonly TorrentFileHelper _torrentFileHelper = new TorrentFileHelper();
        private readonly ITorrentClient _torrentClient = DI.Get<ITorrentClient>();

        [Then(@"I see an overview containing 0 torrents")]
        public void ThenISeeAnOverviewOfTheTorrents()
        {
            InnerAsync().GetAwaiter().GetResult();

            async Task InnerAsync()
            {
                var page = WebDriver.CurrentPageAs<TorrentOverviewPage>();
                await page.InitializeAsync().ConfigureAwait(false);
                page.Torrents.Count.Should().Be(0);
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
            }
        }

        private async Task CreateAndAddTorrentAsync(StageTorrentDto torrentDto)
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            var torrentFilePath = Path.Combine(tempDir, torrentDto.Name + ".torrent");
            Directory.CreateDirectory(tempDir);

            var torrentFileMappings = await CreateTorrentFilesAsync(torrentDto, tempDir).ConfigureAwait(false);

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

            foreach (var torrentFile in torrentFiles)
            {
                var subDirs = Path.GetDirectoryName(torrentFile.FilePath);

                if (!string.IsNullOrWhiteSpace(subDirs))
                {
                    Directory.CreateDirectory(Path.Combine(tempDir, subDirs));
                }

                var torrentFilePath = Path.Combine(tempDir, torrentFile.FilePath);
                await _torrentFileHelper.CreateTextFileAsync(torrentFilePath, (int)torrentFile.SizeInBytes);

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
            var expectedTorrents = table.CreateSet<TorrentOverviewRowDto>().ToList();
            var page = WebDriver.CurrentPageAs<TorrentOverviewPage>();
            var actualTorrents = page.Torrents;

            AssertTorrents(actualTorrents, expectedTorrents);
        }

        private static void AssertTorrents(IEnumerable<Pages.Components.TorrentOverview.TorrentComponent> actualTorrents,
            IEnumerable<TorrentOverviewRowDto> expectedTorrents)
        {
            actualTorrents.Count().Should().Be(expectedTorrents.Count());

            foreach (var expectedTorrent in expectedTorrents)
            {
                var actualTorrent = actualTorrents.Single(p => p.Name == expectedTorrent.Name);
                actualTorrent.GBsOnDisk.Should().Be(expectedTorrent.GBsOnDisk);
                actualTorrent.Location.Should().Be(expectedTorrent.LocationOnDisk);
                actualTorrent.SizeInGB.Should().Be(expectedTorrent.TotalSizeInGB);
                actualTorrent.TotalUploadInGB.Should().Be(expectedTorrent.TotalUploadedInGB);

                AssertTrackers(expectedTorrent, actualTorrent);
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
    }
}
