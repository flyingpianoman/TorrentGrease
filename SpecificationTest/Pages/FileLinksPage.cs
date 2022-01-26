using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using SpecificationTest.Crosscutting;
using SpecificationTest.Pages.Components;
using SpecificationTest.Pages.Components.FileLinks;
using SpecificationTest.Pages.Components.FileManagement;
using SpecificationTest.Pages.Components.PolicyOverview;

namespace SpecificationTest.Pages
{
    class FileLinksPage : PageBase, IPageWithMinFileSize
    {
        private IWebElement _rootElement;
        private IWebElement _container;
        private IWebElement _minFileSizeNr;
        private RadioComponent _minFileSizeUnit;
        public IWebElement AddDirsButton { get; private set; }
        public IWebElement ScanButton { get; private set; }

        public int MinFileSize
        {
            get => Convert.ToInt32(_minFileSizeNr.Text);
            set => _minFileSizeNr.SendKeys(value.ToString());
        }

        public RadioComponent MinFileSizeUnit
        {
            get => _minFileSizeUnit;
        }

        public FileLinksPage(IWebDriver webDriver)
            : base(webDriver)
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync().ConfigureAwait(false);
            _rootElement = _webDriver.WaitForWebElementByContentName("file-links");
            _container = _rootElement.WaitForWebElementByContentName("file-links-container");

            _minFileSizeNr = _container.WaitForWebElementByContentName("min-file-size");
            var fileSizeUnitWebEl = _container.WaitForWebElementByContentName("min-file-size-unit-type");
            _minFileSizeUnit = await new RadioComponent(fileSizeUnitWebEl, _webDriver).InitializeAsync();
            AddDirsButton = _container.WaitForWebElementByContentName("add-dir-button");
            ScanButton = _container.WaitForWebElementByContentName("scan-button");
        }

        public IList<IWebElement> GetDirs()
        {
            return _container.FindElementsByContentName("dir-row").Select(el => el.FindElementByContentName("dir")).ToList();
        }

        internal async Task<FileLinkSelectorComponent> GetFileLinkCandidatesSelectorAsync()
        {
            var c = new FileLinkSelectorComponent(_rootElement, _webDriver);
            return await c.InitializeAsync().ConfigureAwait(false);
        }
    }
}
