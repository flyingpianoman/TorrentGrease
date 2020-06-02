using FluentAssertions;
using OpenQA.Selenium;
using SpecificationTest.Steps.Models;
using SpecificationTest.Pages.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpecificationTest.Crosscutting;

namespace SpecificationTest.Pages
{
    internal sealed class PoliciesOverviewPage : PageBase
    {
        public PoliciesOverviewPage(IWebDriver webDriver)
            : base(webDriver)
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync().ConfigureAwait(false);

            var policiesCardContainer = _webDriver.WaitForWebElementByContentName("policy-card-container");
            var policyCards = policiesCardContainer.FindElementsByContentName("policy");
            
            Policies = policyCards
                .Select(card => new Components.PolicyOverview.PolicyComponent(card))
                .ToList();

            await Policies.InitializeAsync();
        }

        public IList<Components.PolicyOverview.PolicyComponent> Policies { get; private set; }
    }
}
