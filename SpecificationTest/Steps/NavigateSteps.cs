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

namespace SpecificationTest.Steps
{
    [Binding]
    public class NavigateSteps : StepsBase
    {
        [When(@"I navigate to the root url")]
        [When(@"I navigate to the policy overview")]
        public async Task NavigateToTheRootUrl()
        {
            await WebDriver
                .NavigateToPageAsync<PoliciesOverviewPage>(TestSettings.TorrentGreaseDockerAddress);
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
