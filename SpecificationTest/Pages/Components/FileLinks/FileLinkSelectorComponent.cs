using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using SpecificationTest.Crosscutting;

namespace SpecificationTest.Pages.Components.FileLinks
{
    internal class FileLinkSelectorComponent : IComponent<FileLinkSelectorComponent>
    {
        private readonly IWebElement _parentElement;
        private readonly IWebDriver _webDriver;
        private IWebElement _rootElement;

        public FileLinkSelectorComponent(IWebElement parentElement, IWebDriver webDriver)
        {
            _parentElement = parentElement;
            this._webDriver = webDriver;
        }

        public IReadOnlyList<FileLinkCandidateComponent> Candidates { get; private set; }
        public bool IsNoCandidatesFoundMessageVisible => _rootElement.FindElementsByContentName("no-file-link-candidates-found-msg", mustBeDisplayed: true).Any();

        public IWebElement CreateFileLinksButton => _rootElement.FindElementByContentName("create-file-links-button");

        public async Task<FileLinkSelectorComponent> InitializeAsync()
        {
            _rootElement = _parentElement.WaitForWebElementByContentName("file-link-selector-modal");

            var (el, contentName) = _rootElement.WaitForAnyDisplayedWebElementByContentName("file-link-candidate-container", "no-file-link-candidates-found-msg");

            if(contentName == "no-file-link-candidates-found-msg")
            {
                Candidates = new List<FileLinkCandidateComponent>(0);
                return this;
            }

            var candidatesTable = _rootElement.WaitForWebElementByContentName("file-link-candidate-container", mustBeDisplayed: true);
            var candidateRows = candidatesTable.FindElementsByContentName("link-candidate");

            this.Candidates = await candidateRows
                .Select(async el => await new FileLinkCandidateComponent(el, _webDriver).InitializeAsync())
                .AwaitAllToListAsync().ConfigureAwait(false);

            return this;
        }

        internal void WaitToClose()
        {
            _parentElement.WaitForWebElementToCloseByContentName("file-link-selector-modal");
        }
    }
}
