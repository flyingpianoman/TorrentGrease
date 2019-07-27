using FluentAssertions;
using SpecificationTest.Crosscutting;
using SpecificationTest.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SpecificationTest.Steps
{
    [Binding]
    public class NavigateSteps
    {
        [When(@"I navigate to the root url")]
        public async Task WhenINavigateToTheRootUrl()
        {
            await TestRunContext.WebDriver
                .NavigateToPageAsync<PoliciesOverviewPage>(TestSettings.TorrentGreaseDockerAddress);
        }

        [Then(@"I can see the navigation menu")]
        public void ThenICanSeeTheNavigationMenu()
        {
            TestRunContext.WebDriver
                .CurrentPageAs<PageBase>()
                .NavigationMenu
                .NavMenuLinks
                .Any().Should().BeTrue();
        }
    }
}
