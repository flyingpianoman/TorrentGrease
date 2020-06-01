using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecificationTest.Pages.Components.PolicyOverview
{
    class PolicyComponent : IComponent<PolicyComponent>
    {
        private readonly IWebElement _policyWebElement;

        public string Name { get; private set; }
        public string Description { get; private set; }
        public List<TrackerComponent> Trackers { get; private set; }

        public PolicyComponent(IWebElement policyWebElement)
        {
            _policyWebElement = policyWebElement;
        }

        public async Task<PolicyComponent> InitializeAsync()
        {
            Name = _policyWebElement.FindElement(By.CssSelector("*[data-content='title']")).Text;
            Description = _policyWebElement.FindElement(By.CssSelector("*[data-content='description']")).Text;
            Trackers = _policyWebElement.FindElements(By.CssSelector("*[data-content='tracker']"))
                .Select(el => new TrackerComponent(el))
                .ToList();

            await Trackers.InitializeAsync();
            return this;
        }
    }
}
