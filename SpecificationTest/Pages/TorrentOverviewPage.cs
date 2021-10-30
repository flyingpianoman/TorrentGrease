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

        public IWebElement RefreshButton { get; private set; }
        public IWebElement ShowRelocateTorrentsModalButton { get; set; }
        public IWebElement ReAddButton { get; private set; }
        private IWebElement IsInWaitModeInput { get; set; }

        public TorrentOverviewPage(IWebDriver webDriver)
            : base(webDriver)
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync().ConfigureAwait(false);
            _rootElement = _webDriver.WaitForWebElementByContentName("torrent-overview");

            RefreshButton = _webDriver.FindElementByContentName("reload-torrents-button");
            ShowRelocateTorrentsModalButton = _webDriver.FindElementByContentName("show-relocate-torrents-button");
            ReAddButton = _webDriver.FindElementByContentName("re-add-button");
            IsInWaitModeInput = _webDriver.FindElementByContentName("is-in-wait-mode-value");
        }

        public async Task<FiltersPanelComponent> GetFiltersPanelComponentAsync()
        {
            var filtersContainerEl = _rootElement.WaitForWebElementByContentName("filters-collapse");
            var filtersPanel = new FiltersPanelComponent(filtersContainerEl, _webDriver);
            await filtersPanel.InitializeAsync().ConfigureAwait(false);
            return filtersPanel;
        }

        public async Task<IEnumerable<TorrentComponent>> GetTorrentComponentsAsync()
        {
            var torrentsContainer = _rootElement.WaitForWebElementByContentName("torrents-container");
            var torrents = torrentsContainer.FindElementsByContentName("torrent");

            var torrentsComp = torrents
                .Select(torrent => new TorrentComponent(torrent, _webDriver))
                .ToList();

            await torrentsComp.InitializeAsync();
            return torrentsComp;
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

        internal async Task<FiltersPanelComponent> EnsureFilterAccordionIsCollapsedAsync()
        {
            var filtersPanel = await GetFiltersPanelComponentAsync().ConfigureAwait(false);
            if(!filtersPanel.IsExpanded)
            {
                filtersPanel.Expand();
            }

            return filtersPanel;
        }

        internal void WaitUntilNotInWaitMode()
        {
            PageHelper.WaitForWebElementPolicy.Execute(() =>
            {
                if (this.IsInWaitModeInput.GetAttribute("value") != "false")
                {
                    throw new PageHelper.RetryException();
                }
            });
        }
    }
}
