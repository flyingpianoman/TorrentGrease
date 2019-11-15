using FluentAssertions;
using SpecificationTest.Crosscutting;
using SpecificationTest.Pages;
using SpecificationTest.Steps.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace SpecificationTest.Steps
{
    [Binding]
    public class PoliciesOverviewSteps : StepsBase
    {
        [Then(@"I see an overview of all policies")]
        public void ThenISeeAnOverviewOfAllPolicies()
        {
            var page = WebDriver.CurrentPageAs<PoliciesOverviewPage>();
            page.Policies.Should().NotBeNull();
        }

        [Then(@"I see an overview of the following policies")]
        public void ThenISeeAnOverviewOfTheFollowingPolicies(Table table)
        {
            var page = WebDriver.CurrentPageAs<PoliciesOverviewPage>();
            var actualPolicies = page.Policies;
            var expectedPolicies = table.CreateSet<PolicyOverviewRowDto>().ToList();
            actualPolicies.Count.Should().Be(expectedPolicies.Count);

            foreach (var expectedRow in expectedPolicies)
            {
                var actualRow = actualPolicies.Single(p => p.Name == expectedRow.Name);
                actualRow.Should().NotBeNull();
            }
        }
    }
}
