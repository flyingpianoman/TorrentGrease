using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using SpecificationTest.Crosscutting;

namespace SpecificationTest.Pages.Components.FileManagement
{
    internal class FileRemovalSelectorComponent : IComponent<FileRemovalSelectorComponent>
    {
        private readonly IWebElement _parentElement;
        private readonly IWebDriver _webDriver;
        private IWebElement _rootElement;

        public FileRemovalSelectorComponent(IWebElement parentElement, IWebDriver webDriver)
        {
            _parentElement = parentElement;
            this._webDriver = webDriver;
        }

        public IReadOnlyList<FileRemovalCandidateComponent> FileRemovalCandidates { get; private set; }

        public IWebElement RemoveFilesButton => _rootElement.FindElementByContentName("remove-selected-orphan-files-button");

        public async Task<FileRemovalSelectorComponent> InitializeAsync()
        {
            _rootElement = _parentElement.WaitForWebElementByContentName("file-removal-selector-torrents-modal");
            var fileRemovalCandidatesTable = _rootElement.WaitForWebElementByContentName("file-removal-candidate-picker", mustBeDisplayed: true);
            var candidateRows = fileRemovalCandidatesTable.FindElementsByContentName("relocate-candidate");

            this.FileRemovalCandidates = await candidateRows
                .Select(async el => await new FileRemovalCandidateComponent(el, _webDriver).InitializeAsync())
                .AwaitAllToListAsync().ConfigureAwait(false);

            return this;
        }

        internal void WaitToClose()
        {
            _parentElement.WaitForWebElementToCloseByContentName("file-removal-selector-torrents-modal");
        }
    }
}
