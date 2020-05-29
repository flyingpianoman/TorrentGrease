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

namespace SpecificationTest.Pages
{
    internal sealed class TorrentOverviewPage : PageBase
    {
        public TorrentOverviewPage(IWebDriver webDriver)
            : base(webDriver)
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync().ConfigureAwait(false);

            await InitializeTorrentsAsync().ConfigureAwait(false);
            ShowRemapTorrentsModalButton = _webDriver.FindElement(By.CssSelector("*[data-content='show-remap-torrents-button']"));
        }

        private async Task InitializeTorrentsAsync()
        {
            var torrentsContainer = PageHelper.WaitForWebElementPolicy
                .Execute(() =>
                {
                    var elements = _webDriver.FindElements(By.CssSelector("*[data-content='torrents-container']"));
                    elements.Count.Should().Be(1);

                    return elements[0];
                });

            var torrents = torrentsContainer.FindElements(By.CssSelector("*[data-content='torrent']"));

            Torrents = torrents
                .Select(torrent => new TorrentComponent(torrent))
                .ToList();

            await Torrents.InitializeAsync();
        }

        public async Task<RemapTorrentsLocationDialogComponent> GetRemapTorrentsLocationDialogComponentAsync()
        {
            var dialog = PageHelper.WaitForWebElementPolicy
                .Execute(() =>
                {
                    var elements = _webDriver.FindElements(By.CssSelector("*[data-content='remap-torrents-modal']"));
                    elements.Count.Should().Be(1);

                    return new RemapTorrentsLocationDialogComponent(elements[0]);
                });

            await dialog.InitializeAsync().ConfigureAwait(false);

            return dialog;
        }

        public IWebElement ShowRemapTorrentsModalButton { get; set; }
        public IList<TorrentComponent> Torrents { get; private set; }
    }
}
