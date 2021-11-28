using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using SpecificationTest.Crosscutting;
using SpecificationTest.Pages.Components;
using SpecificationTest.Pages.Components.PolicyOverview;

namespace SpecificationTest.Pages
{
    class FileManagementPage : PageBase
    {
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
            _fileMngmtContainer = _webDriver.WaitForWebElementByContentName("file-management");

            _minFileSizeNr = _fileMngmtContainer.WaitForAnyWebElementByContentName("min-file-size");
            var fileSizeUnitWebEl = _fileMngmtContainer.WaitForAnyWebElementByContentName("min-file-size-unit-type");
            _minFileSizeUnit = await new RadioComponent(fileSizeUnitWebEl).InitializeAsync();
            AddCompletedTorrentDirMappingButton = _fileMngmtContainer.WaitForAnyWebElementByContentName("add-completed-torrent-dir-mapping-button");
            _scanButton = _fileMngmtContainer.WaitForAnyWebElementByContentName("scan-button");
        }

        public async Task<IList<CompletedDirMappingComponent>> GetCompletedDirMappingsAsync()
        {
            return (await Task.WhenAll(_fileMngmtContainer.FindElementsByContentName("completed-torrent-dir-mapping-row")
                    .Select(el => new CompletedDirMappingComponent(el).InitializeAsync())).ConfigureAwait(false))
                .ToList();
        }
    }
}
