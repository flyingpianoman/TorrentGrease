using FluentAssertions;
using SpecificationTest.Crosscutting;
using SpecificationTest.Pages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SpecificationTest.Steps
{
    [Binding]
    public class PoliciesSteps
    {
        [Then(@"I see an overview of all policies")]
        public void ThenISeeAnOverviewOfAllPolicies()
        {
            var page = TestRunContext.WebDriver.CurrentPageAs<PoliciesOverviewPage>();
            page.Policies.Should().NotBeNull();
        }

    }
}
