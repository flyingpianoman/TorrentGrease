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
    class FileSizeSelectorSteps : StepsBase
    {
        [Given(@"I set a minimal filesize of (\d+) (B|KB|MB)")]
        public void GivenISetAMinimalFilesizeOfKB(int size, string unitType)
        {
            var page = GetPageWithMinFileSize();

            page.MinFileSize = size;
            page.MinFileSizeUnit.ClickRadioOptionByValue(unitType);
        }

        private IPageWithMinFileSize GetPageWithMinFileSize()
        {
            var basePage = WebDriver.CurrentPageAs<PageBase>();
            return basePage.NavigationMenu.ActiveLink.Target switch
            {
                NavMenuItemTarget.FileManagement => WebDriver.CurrentPageAs<FileManagementPage>(),
                NavMenuItemTarget.FileLinks => WebDriver.CurrentPageAs<FileLinksPage>(),
                _ => throw new NotSupportedException(),
            };
        }
    }
}
