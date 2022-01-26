using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using SpecificationTest.Crosscutting;
using SpecificationTest.Pages;
using SpecificationTest.Steps.Models;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace SpecificationTest.Steps
{
    [Binding]
    class FileManagementSteps : StepsBase
    {

        [Given(@"I set the following torrent dir mapping")]
        public void GivenISetTheFollowingTorrentDirMapping(Table table)
        {
            InnerAsync().GetAwaiter().GetResult();

            async Task InnerAsync()
            {
                //This code assumes a fresh page with 1 row
                var dirMappingDtos = table.CreateSet<CompletedDirMappingDto>().ToList();
                var page = WebDriver.CurrentPageAs<FileManagementPage>();
                var isFirst = true;

                foreach (var dirMappingDto in dirMappingDtos)
                {
                    if (!isFirst)
                    {
                        page.AddCompletedTorrentDirMappingButton.Click();
                    }
                    else
                    {
                        isFirst = false;
                    }

                    var mappingRows = await page.GetCompletedDirMappingsAsync();
                    mappingRows.Last().TorrentClientCompletedDirCsv = dirMappingDto.TorrentClientDirCsv;
                    mappingRows.Last().TorrentGreaseCompletedDir = dirMappingDto.TorrentGreaseDir;
                }
            }
        }

        [When(@"I scan for orphanized files")]
        [Given(@"I scan for orphanized files")]
        public void WhenIScanForOrphanizedFiles()
        {
            var page = WebDriver.CurrentPageAs<FileManagementPage>();
            page.ScanButton.Click();
        }

        [Then(@"I see an overview of the following files")]
        [Given(@"I see an overview of the following files")]
        public void ThenISeeAnOverviewOfTheFollowingFiles(Table table)
        {
            InnerAsync().GetAwaiter().GetResult();
            
            async Task InnerAsync()
            {
                var expectedFileRemovalCandidates = table.CreateSet<FileRemovalCandidateDto>().ToList();

                var page = WebDriver.CurrentPageAs<FileManagementPage>();
                var fileRemovalSelector = await page.GetFileRemovalSelectorComponentAsync();

                fileRemovalSelector.FileRemovalCandidates.Count.Should().Be(expectedFileRemovalCandidates.Count);

                for (int i = 0; i < expectedFileRemovalCandidates.Count; i++)
                {
                    var expected = expectedFileRemovalCandidates[i];
                    var actual = fileRemovalSelector.FileRemovalCandidates[i];

                    actual.FilePath.Should().Be(expected.FilePath);
                    actual.FileSize.Should().Be(expected.Size);
                }
            }
        }

        [Given(@"I select the following orphan files")]
        public void GivenISelectTheFollowingOrphanFiles(Table table)
        {
            InnerAsync().GetAwaiter().GetResult();

            async Task InnerAsync()
            {
                var filesToSelect = table.Rows.Select(r => r.Single().Value).ToList();

                var page = WebDriver.CurrentPageAs<FileManagementPage>();
                var fileRemovalSelector = await page.GetFileRemovalSelectorComponentAsync();

                foreach (var fileRemovalCandidate in fileRemovalSelector.FileRemovalCandidates)
                {
                    fileRemovalCandidate.IsSelected = filesToSelect.Contains(fileRemovalCandidate.FilePath);
                }
            }
        }

        [When(@"I remove the selected orphan files")]
        public void WhenIRemoveTheSelectedOrphanFiles()
        {
            InnerAsync().GetAwaiter().GetResult();

            async Task InnerAsync()
            {
                var page = WebDriver.CurrentPageAs<FileManagementPage>();
                var fileRemovalSelector = await page.GetFileRemovalSelectorComponentAsync();
                fileRemovalSelector.RemoveFilesButton.Click();
                fileRemovalSelector.WaitToClose();
            }
        }

    }
}
