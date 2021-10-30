using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
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
        private readonly Func<Task> _onFilterChangeAsync;

        public FiltersPanelComponent(IWebElement filtersPanelWebElement, IWebDriver webDriver, 
            Func<Task> onFilterChangeAsync)
        {
            _filtersPanelWebElement = filtersPanelWebElement;
            _webDriver = webDriver;
            _onFilterChangeAsync = onFilterChangeAsync;
        }

        private IWebElement _collapseHeader;
        private IWebElement _collapseBody;
        public IEnumerable<CheckboxComponent> ErrorFilterCheckboxes { get; private set; }

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
                UpdateFilters();

                //If no checkboxes, we should have a no errors msg - else retry until either one of them is there
                if (!ErrorFilterCheckboxes.Any() && _collapseBody.FindElementByContentName("no-error-filters-msg") == null)
                {
                    throw new RetryException();
                }
            });
        }

        public void UpdateFilters()
        {
            if (IsExpanded)
            {
                ErrorFilterCheckboxes = _collapseBody
                    .FindElementsByContentName("error-filter-checkbox")
                    .Select(el => new CheckboxComponent(el, _webDriver, _onFilterChangeAsync))
                    .ToArray();
            }
        }

        public Task<FiltersPanelComponent> InitializeAsync()
        {
            _collapseHeader = _filtersPanelWebElement.FindElementByContentName("filters-collapse-header");
            _collapseBody = _filtersPanelWebElement.FindElementByContentName("filters-collapse-body");

            return Task.FromResult(this);
        }
    }
}
