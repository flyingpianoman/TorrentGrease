using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using SpecificationTest.Crosscutting;
using SpecificationTest.Pages;
using SpecificationTest.Pages.Components;
using SpecificationTest.Steps.Models;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

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

        [When(@"I scan for possible file links")]
        public void WhenIScanForPossibleFileLinks()
        {
            var page = WebDriver.CurrentPageAs<FileLinksPage>();
            page.ScanButton.Click();
        }

        [Then(@"I see an overview of the following file link candidates")]
        public void ThenISeeAnOverviewOfTheFollowingFiles(Table table)
        {
            InnerAsync().GetAwaiter().GetResult();
            
            async Task InnerAsync()
            {
                var expectedCandidates = table.CreateSet<FileLinkCandidateDto>().ToList();

                var page = WebDriver.CurrentPageAs<FileLinksPage>();
                var fileRemovalSelector = await page.GetFileLinkCandidatesSelectorAsync();

                fileRemovalSelector.Candidates.Count.Should().Be(expectedCandidates.Count);

                for (int i = 0; i < expectedCandidates.Count; i++)
                {
                    var expected = expectedCandidates[i];
                    var actual = fileRemovalSelector.Candidates[i];

                    //we can loop through the pages if we get a test case for it
                    actual.FilePathsOnCurrentPage.Count().Should().Be(expected.FilePaths.Count());
                    
                    actual.FilePathsOnCurrentPage.Should().BeEquivalentTo(expected.FilePaths);
                    actual.FileSize.Should().Be(expected.Size);
                }
            }
        }

    }
}
