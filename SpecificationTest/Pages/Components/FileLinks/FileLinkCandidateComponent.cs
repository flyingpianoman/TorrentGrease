using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using SpecificationTest.Crosscutting;

namespace SpecificationTest.Pages.Components.FileLinks
{
    internal class FileLinkCandidateComponent : IComponent<FileLinkCandidateComponent>
    {
        private readonly IWebElement _rootElement;
        private IWebElement _selectorWebElement;
        private readonly IWebDriver _webDriver;

        public bool IsSelected
        {
            get => _selectorWebElement.Selected;
            set
            {
                if (IsSelected != value)
                {
                    _selectorWebElement.ClickBootstrapCheckBox(_webDriver);
                }
            }
        }

        public IEnumerable<string> FilePathsOnCurrentPage => _rootElement.FindElementsByContentName("filepath")
            .Select(el => el.Text)
            .ToArray();

        /// <summary>
        /// Size + unit e.g. '25 kb'
        /// </summary>
        public string FileSize => _rootElement.FindElementByContentName("filesize").Text;

        public FileLinkCandidateComponent(IWebElement rootElement, IWebDriver webDriver)
        {
            _rootElement  = rootElement;
            _webDriver = webDriver;
        }

        public Task<FileLinkCandidateComponent> InitializeAsync()
        {
            _selectorWebElement = _rootElement.FindElementByContentName("selector");
            return Task.FromResult(this);
        }
    }
}
