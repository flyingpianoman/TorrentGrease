using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using SpecificationTest.Crosscutting;
using static SpecificationTest.Pages.PageHelper;

namespace SpecificationTest.Pages.Components.TorrentOverview
{
    class FiltersPanelComponent : IComponent<FiltersPanelComponent>
    {
        private readonly IWebElement _filtersPanelWebElement;
        private readonly IWebDriver _webDriver;

        public FiltersPanelComponent(IWebElement filtersPanelWebElement, IWebDriver webDriver)
        {
            _filtersPanelWebElement = filtersPanelWebElement;
            _webDriver = webDriver;
        }

        private IWebElement _collapseHeader;
        private IWebElement _collapseBody;

        public bool IsExpanded
        {
            get
            {
                return _collapseBody.Displayed;
            }
        }

        public void Expand()
        {
            _collapseHeader.Click();
            _filtersPanelWebElement.WaitForWebElementByContentName("filters-collapse-body");

            PageHelper.WaitForWebElementPolicy.Execute(() =>
            {
                var errorFilterCheckboxes = GetErrorFilters();

                //If no checkboxes, we should have a no errors msg - else retry until either one of them is there
                if (!errorFilterCheckboxes.Any() && _collapseBody.FindElementByContentName("no-error-filters-msg") == null)
                {
                    throw new RetryException();
                }
            });
        }

        public IEnumerable<CheckboxComponent> GetErrorFilters()
        {
            return IsExpanded
                    ? _collapseBody
                        .FindElementsByContentName("error-filter-checkbox")
                        .Select(el => new CheckboxComponent(el, _webDriver))
                        .ToArray()
                    : null;
        }

        public Task<FiltersPanelComponent> InitializeAsync()
        {
            _collapseHeader = _filtersPanelWebElement.FindElementByContentName("filters-collapse-header");
            _collapseBody = _filtersPanelWebElement.FindElementByContentName("filters-collapse-body");

            return Task.FromResult(this);
        }
    }
}
