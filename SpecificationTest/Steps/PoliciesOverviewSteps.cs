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
            var expectedPolicies = table.CreateSet<PolicyOverviewRowDto>().ToList();
            var page = WebDriver.CurrentPageAs<PoliciesOverviewPage>();
            var actualPolicies = page.Policies;
            actualPolicies.Count.Should().Be(expectedPolicies.Count);

            foreach (var expectedPolicy in expectedPolicies)
            {
                AssertPolicy(actualPolicies, expectedPolicy);
            }
        }

        private static void AssertPolicy(IList<Pages.Components.PolicyOverview.PolicyComponent> actualPolicies, PolicyOverviewRowDto expectedPolicy)
        {
            var actualPolicy = actualPolicies.Single(p => p.Name == expectedPolicy.Name);
            actualPolicy.Description.Should().Be(expectedPolicy.Description);

            AssertPolicyTrackers(expectedPolicy, actualPolicy);
        }

        private static void AssertPolicyTrackers(PolicyOverviewRowDto expectedPolicy, Pages.Components.PolicyOverview.PolicyComponent actualPolicy)
        {
            if (!String.IsNullOrEmpty(expectedPolicy.Tracker1))
            {
                actualPolicy.Trackers[0].Name.Should().Be(expectedPolicy.Tracker1);
            }
            if (!String.IsNullOrEmpty(expectedPolicy.Tracker2))
            {
                actualPolicy.Trackers[1].Name.Should().Be(expectedPolicy.Tracker2);
            }
        }
    }
}
