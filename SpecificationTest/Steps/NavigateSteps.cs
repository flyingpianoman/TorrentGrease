using FluentAssertions;
using OpenQA.Selenium;
using SpecificationTest.Crosscutting;
using SpecificationTest.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using TestUtils;

namespace SpecificationTest.Steps
{
    [Binding]
    public class NavigateSteps : StepsBase
    {
        [When(@"I navigate to the root url")]
        [When(@"I navigate to the policy overview")]
        public async Task NavigateToTheRootUrl()
        {
            await NavigateToRootUrlAsync().ConfigureAwait(false);
        }

        private async Task<PoliciesOverviewPage> NavigateToRootUrlAsync()
        {
            return await WebDriver
                    .NavigateToPageAsync<PoliciesOverviewPage>(TestSettings.TorrentGreaseDockerAddress).ConfigureAwait(false);
        }

        [Given(@"I navigate to the torrent overview")]
        [When(@"I navigate to the torrent overview")]
        public async Task NavigateToTheTorrentOverview()
        {
            await (await NavigateToRootUrlAsync().ConfigureAwait(false))
                .NavigationMenu
                .TorrentsNavMenuLink
                .NavigateAsync<TorrentOverviewPage>()
                .ConfigureAwait(false);
        }

        [Then(@"I can see the navigation menu")]
        public void ThenICanSeeTheNavigationMenu()
        {
            WebDriver
                .CurrentPageAs<PageBase>()
                .NavigationMenu
                .NavMenuLinks
                .Any().Should().BeTrue();
        }
    }
}
