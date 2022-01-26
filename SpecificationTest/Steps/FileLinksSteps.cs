using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Docker.DotNet;
using FluentAssertions;
using SpecificationTest.Crosscutting;
using SpecificationTest.Pages;
using SpecificationTest.Pages.Components;
using SpecificationTest.Steps.Models;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using TestUtils;

namespace SpecificationTest.Steps
{
    [Binding]
    class FileLinksSteps : StepsBase
    {

        [Given(@"I set the following directories to scan")]
        public void GivenISetTheFollowingDirectoriesToScan(Table table)
        {
            var dirs = table.Rows.Select(r => r.Values.Single()).ToArray();
            var page = WebDriver.CurrentPageAs<FileLinksPage>();
            var isFirst = true;

            foreach (var dir in dirs)
            {
                if (!isFirst)
                {
                    page.AddDirsButton.Click();
                }
                else
                {
                    isFirst = false;
                }

                var dirElements = page.GetDirs();
                dirElements.Last().SendKeys(dir);
            }
        }

        [Given(@"I scan for possible file links")]
        [When(@"I scan for possible file links")]
        [Then(@"when I scan for possible file links again")]
        public void WhenIScanForPossibleFileLinks()
        {
            var page = WebDriver.CurrentPageAs<FileLinksPage>();
            page.ScanButton.Click();
        }

        [Given(@"I see an overview of the following file link candidates")]
        [Then(@"I see an overview of the following file link candidates")]
        public void ThenISeeAnOverviewOfTheFollowingFiles(Table table)
        {
            InnerAsync().GetAwaiter().GetResult();
            
            async Task InnerAsync()
            {
                var expectedCandidates = table.CreateSet<FileLinkCandidateDto>().ToList();

                var page = WebDriver.CurrentPageAs<FileLinksPage>();
                var fileLinksSelector = await page.GetFileLinkCandidatesSelectorAsync();

                fileLinksSelector.Candidates.Count.Should().Be(expectedCandidates.Count);

                for (int i = 0; i < expectedCandidates.Count; i++)
                {
                    var expected = expectedCandidates[i];
                    var actualCandidate = fileLinksSelector.Candidates[i];

                    //we can loop through the pages if we get a test case for it
                    var actualCandidateFiles = await actualCandidate.GetFilesOnCurrentPageAsync();
                    actualCandidateFiles.Count().Should().Be(expected.FilePaths.Count());

                    actualCandidateFiles.Select(cf => cf.FilePath).Should().BeEquivalentTo(expected.FilePaths);
                    actualCandidate.FileSize.Should().Be(expected.Size);
                }
            }
        }

        [Then(@"I see an empty file link candidates overview")]
        public void ThenISeeAnEmptyFileLinkCandidatesOverview()
        {
            InnerAsync().GetAwaiter().GetResult();

            async Task InnerAsync()
            {
                var page = WebDriver.CurrentPageAs<FileLinksPage>();
                var fileLinksSelector = await page.GetFileLinkCandidatesSelectorAsync();
                fileLinksSelector.IsNoCandidatesFoundMessageVisible.Should().BeTrue();
                fileLinksSelector.Candidates.Count.Should().Be(0);
            }
        }

        /// <summary>
        /// Works only on single page lists atm
        /// </summary>
        [When(@"I ensure that only the candidates containing the following paths are selected")]
        public void WhenIEnsureThatOnlyTheCandidatesContainingTheFollowingPathsAreSelected(Table table)
        {
            InnerAsync().GetAwaiter().GetResult();

            async Task InnerAsync()
            {
                var filePaths = table.Rows.Select(r => r.Values.First()).ToArray();
                var page = WebDriver.CurrentPageAs<FileLinksPage>();
                var fileLinksSelector = await page.GetFileLinkCandidatesSelectorAsync();

                foreach (var candidate in fileLinksSelector.Candidates)
                {
                    var candidateFiles = await candidate.GetFilesOnCurrentPageAsync();
                    candidate.IsSelected = candidateFiles.Any(f => filePaths.Contains(f.FilePath));
                }
            }
        }

        [When(@"I create file links for the selected candidates")]
        public void WhenICreateFileLinksForTheSelectedCandidates()
        {
            InnerAsync().GetAwaiter().GetResult();

            async Task InnerAsync()
            {
                var page = WebDriver.CurrentPageAs<FileLinksPage>();
                var fileLinksSelector = await page.GetFileLinkCandidatesSelectorAsync();
                fileLinksSelector.CreateFileLinksButton.Click();
                fileLinksSelector.WaitToClose();
            }
        }

        [Then(@"the following files are all linked to the same device and inode")]
        public void ThenTheFollowingFilesAreAllLinkedToTheSameDeviceAndInode(Table table)
        {
            InnerAsync().GetAwaiter().GetResult();

            async Task InnerAsync()
            {
                var dockerClient = DI.Get<DockerClient>();
                var id = await dockerClient.Containers.GetContainerIdByNameAsync(TestSettings.TorrentGreaseContainerName);
                var filePaths = table.Rows.Select(r => r.Values.First()).ToArray();

                var fileInfos = await filePaths
                    .Select(async fp => await dockerClient.GetLinuxFileInfoInContainerAsync(id, fp))
                    .AwaitAllAsync();

                fileInfos.Should().HaveCount(filePaths.Length);

                //Should all have the same inode
                fileInfos
                    .DistinctBy(lfi => (lfi.DeviceId, lfi.InodeId))
                    .Should().HaveCount(1);
            }
        }

    }
}
