using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;

namespace SpecificationTest.Pages.Components.TorrentOverview
{
    class RelocateTorrentsLocationDialogComponent : IComponent
    {
        private readonly IWebElement _relocateTorrentsLocationDialogWebElement;

        public RelocateTorrentsLocationDialogComponent(IWebElement relocateTorrentsLocationDialogWebElement)
        {
            _relocateTorrentsLocationDialogWebElement = relocateTorrentsLocationDialogWebElement;
        }

        public IEnumerable<IWebElement> PathToScanElements { get; set; }
        public IWebElement RelocateTorrentsButton { get; set; }
        private IWebElement AddPathToScanButton { get; set; }

        public void ClickAddPathToScanButton()
        {
            AddPathToScanButton.Click();
            LoadPathToScanInputs();
        }

        public Task InitializeAsync()
        {
            LoadPathToScanInputs();

            RelocateTorrentsButton = _relocateTorrentsLocationDialogWebElement.FindElement(By.CssSelector("*[data-content='show-relocate-torrents-button']"));
            AddPathToScanButton = _relocateTorrentsLocationDialogWebElement.FindElement(By.CssSelector("*[data-content='add-path-to-scan-button']"));

            return Task.CompletedTask;
        }

        private void LoadPathToScanInputs()
        {
            PathToScanElements = _relocateTorrentsLocationDialogWebElement.FindElements(By.CssSelector("*[data-content='path-to-scan']"));
        }
    }
}
