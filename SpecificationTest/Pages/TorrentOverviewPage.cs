using FluentAssertions;
using OpenQA.Selenium;
using SpecificationTest.Steps.Models;
using SpecificationTest.Pages.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpecificationTest.Pages.Components.TorrentOverview;
using SpecificationTest.Crosscutting;

namespace SpecificationTest.Pages
{
    internal sealed class TorrentOverviewPage : PageBase
    {
        private IWebElement _rootElement;

        public TorrentOverviewPage(IWebDriver webDriver)
            : base(webDriver)
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync().ConfigureAwait(false);
            _rootElement = _webDriver.WaitForWebElementByContentName("torrent-overview");

            await InitializeTorrentsAsync().ConfigureAwait(false);
            RefreshButton = _webDriver.FindElementByContentName("reload-torrents-button");
            ShowRelocateTorrentsModalButton = _webDriver.FindElementByContentName("show-relocate-torrents-button");
        }

        private async Task InitializeTorrentsAsync()
        {
            var torrentsContainer = _rootElement.WaitForWebElementByContentName("torrents-container");
            var torrents = torrentsContainer.FindElementsByContentName("torrent");

            Torrents = torrents
                .Select(torrent => new TorrentComponent(torrent, _webDriver))
                .ToList();

            await Torrents.InitializeAsync();
        }

        public async Task<ScanForRelocationCandidatesComponent> GetScanForRelocationCandidatesComponentAsync()
        {
            var dialog = new ScanForRelocationCandidatesComponent(_rootElement);
            return await dialog.InitializeAsync().ConfigureAwait(false);
        }


        public async Task<PickRelocationCandidatesComponent> GetPickRelocationCandidatesComponentAsync()
        {
            var dialog = new PickRelocationCandidatesComponent(_rootElement, _webDriver);
            return await dialog.InitializeAsync().ConfigureAwait(false);
        }

        private IWebElement RefreshButton { get;  set; }
        public async Task RefreshTorrentsAsync()
        {
            RefreshButton.Click();
            //Maybe we need to figure out how to first see the loading animation, but I think we don't
            await InitializeTorrentsAsync().ConfigureAwait(false);
        }

        public IWebElement ShowRelocateTorrentsModalButton { get; set; }
        public IList<TorrentComponent> Torrents { get; private set; }
    }
}
