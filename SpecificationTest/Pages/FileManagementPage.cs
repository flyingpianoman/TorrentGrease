using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using SpecificationTest.Crosscutting;
using SpecificationTest.Pages.Components;
using SpecificationTest.Pages.Components.FileManagement;
using SpecificationTest.Pages.Components.PolicyOverview;

namespace SpecificationTest.Pages
{
    class FileManagementPage : PageBase, IPageWithMinFileSize
    {
        private IWebElement _rootElement;
        private IWebElement _fileMngmtContainer;
        private IWebElement _minFileSizeNr;
        private RadioComponent _minFileSizeUnit;
        public IWebElement AddCompletedTorrentDirMappingButton { get; private set; }
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

        public FileManagementPage(IWebDriver webDriver)
            : base(webDriver)
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync().ConfigureAwait(false);
            _rootElement = _webDriver.WaitForWebElementByContentName("file-management");
            _fileMngmtContainer = _rootElement.WaitForWebElementByContentName("file-management-container");

            _minFileSizeNr = _fileMngmtContainer.WaitForAnyWebElementByContentName("min-file-size");
            var fileSizeUnitWebEl = _fileMngmtContainer.WaitForAnyWebElementByContentName("min-file-size-unit-type");
            _minFileSizeUnit = await new RadioComponent(fileSizeUnitWebEl, _webDriver).InitializeAsync();
            AddCompletedTorrentDirMappingButton = _fileMngmtContainer.WaitForAnyWebElementByContentName("add-completed-torrent-dir-mapping-button");
            ScanButton = _fileMngmtContainer.WaitForAnyWebElementByContentName("scan-button");
        }

        public async Task<IList<CompletedDirMappingComponent>> GetCompletedDirMappingsAsync()
        {
            return (await Task.WhenAll(_fileMngmtContainer.FindElementsByContentName("completed-torrent-dir-mapping-row")
                    .Select(el => new CompletedDirMappingComponent(el).InitializeAsync())).ConfigureAwait(false))
                .ToList();
        }

        internal async Task<FileRemovalSelectorComponent> GetFileRemovalSelectorComponentAsync()
        {
            var c = new FileRemovalSelectorComponent(_rootElement, _webDriver);
            return await c.InitializeAsync().ConfigureAwait(false);
        }
    }
}
