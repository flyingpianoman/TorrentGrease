using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;

namespace SpecificationTest.Pages.Components.TorrentOverview
{
    class RemapTorrentsLocationDialogComponent : IComponent
    {
        private readonly IWebElement _remapTorrentsLocationDialogWebElement;

        public RemapTorrentsLocationDialogComponent(IWebElement remapTorrentsLocationDialogWebElement)
        {
            _remapTorrentsLocationDialogWebElement = remapTorrentsLocationDialogWebElement;
        }

        public IEnumerable<IWebElement> PathToScanElements { get; set; }
        public IWebElement RemapTorrentsButton { get; set; }
        public IWebElement AddPathToScanButton { get; set; }

        public Task InitializeAsync()
        {
            PathToScanElements = _remapTorrentsLocationDialogWebElement.FindElements(By.CssSelector("*[data-content='path-to-scan']"));
            RemapTorrentsButton = _remapTorrentsLocationDialogWebElement.FindElement(By.CssSelector("*[data-content='show-remap-torrents-button']"));
            AddPathToScanButton = _remapTorrentsLocationDialogWebElement.FindElement(By.CssSelector("*[data-content='add-path-to-scan-button']"));

            return Task.CompletedTask;
        }
    }
}
