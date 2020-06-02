using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using SpecificationTest.Crosscutting;

namespace SpecificationTest.Pages.Components.TorrentOverview
{
    class PickRelocationCandidatesComponent : IComponent<PickRelocationCandidatesComponent>
    {
        const string RootElementName = "relocate-candidates-picker";
        private readonly IWebElement _parentElement;
        private readonly IWebDriver _webDriver;
        private IWebElement _rootElement;

        public PickRelocationCandidatesComponent(IWebElement parentElement, IWebDriver webDriver)
        {
            _parentElement = parentElement;
            _webDriver = webDriver;
        }

        private IWebElement VerifyTorrentsCheckBox { get; set; }
        public bool IsVerifyTorrentsEnabled
        {
            get
            {
                return VerifyTorrentsCheckBox.Selected;
            }
            set
            {
                if (IsVerifyTorrentsEnabled != value)
                {
                    VerifyTorrentsCheckBox.ClickBootstrapCheckBox(_webDriver);
                }
            }
        }

        public IWebElement RelocateCandidatesButton { get; private set; }
        public TorrentRelocationCandidateComponent[] TorrentRelocationCandidates { get; private set; }

        public async Task<PickRelocationCandidatesComponent> InitializeAsync()
        {
            _rootElement = _parentElement.WaitForWebElementByContentName(RootElementName);

            VerifyTorrentsCheckBox = _rootElement.FindElementByContentName("verify-torrents-after-relocating");
            var candidatesContainer = _rootElement.FindElementByContentName("relocate-candidates-container");

            var initCandidatesQuery = candidatesContainer.FindElementsByContentName("relocate-candidate")
                .Select(async e => await new TorrentRelocationCandidateComponent(e, _webDriver).InitializeAsync().ConfigureAwait(false));
            TorrentRelocationCandidates = await Task.WhenAll(initCandidatesQuery).ConfigureAwait(false);

            RelocateCandidatesButton = _parentElement.FindElementByContentName("relocate-candidates-button");

            return this;
        }

        internal void WaitUntilClosed()
        {
            _parentElement.WaitForWebElementToCloseByContentName(RootElementName);
        }
    }
}
