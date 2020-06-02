using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using OpenQA.Selenium;
using SpecificationTest.Crosscutting;

namespace SpecificationTest.Pages.Components.TorrentOverview
{
    class ScanForRelocationCandidatesComponent : IComponent<ScanForRelocationCandidatesComponent>
    {
        private readonly IWebElement _parentElement;
        private IWebElement _rootElement;

        public ScanForRelocationCandidatesComponent(IWebElement parentElement)
        {
            _parentElement = parentElement;
        }

        public IEnumerable<IWebElement> PathToScanElements { get; set; }
        public IWebElement ScanForCandidatesButton { get; set; }
        private IWebElement AddPathToScanButton { get; set; }

        public void ClickAddPathToScanButton()
        {
            AddPathToScanButton.Click();
            LoadPathToScanInputs();
        }

        public void ClickScanForCandidatesButton()
        {
            ScanForCandidatesButton.Click();
        }

        public Task<ScanForRelocationCandidatesComponent> InitializeAsync()
        {
            _rootElement = _parentElement.WaitForWebElementByContentName("find-relocate-candidates");

            LoadPathToScanInputs();

            ScanForCandidatesButton = _parentElement.FindElement(By.CssSelector("*[data-content='scan-for-candidates-button']"));
            AddPathToScanButton = _rootElement.FindElement(By.CssSelector("*[data-content='add-path-to-scan-button']"));

            return Task.FromResult(this);
        }

        private void LoadPathToScanInputs()
        {
            PathToScanElements = _rootElement.FindElements(By.CssSelector("*[data-content='path-to-scan']"));
        }
    }
}
