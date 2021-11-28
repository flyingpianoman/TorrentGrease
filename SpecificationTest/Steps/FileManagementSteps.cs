using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        [Given(@"I set a minimal filesize of (\d+) (B|KB|MB)")]
        public void GivenISetAMinimalFilesizeOfKB(int size, string unitType)
        {
            var page = WebDriver.CurrentPageAs<FileManagementPage>();
            page.MinFileSize = size;
            page.MinFileSizeUnit.ClickRadioOptionByValue(unitType);
        }

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

    }
}
