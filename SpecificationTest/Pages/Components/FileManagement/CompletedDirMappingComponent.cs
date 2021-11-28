using OpenQA.Selenium;
using SpecificationTest.Crosscutting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecificationTest.Pages.Components.PolicyOverview
{
    class CompletedDirMappingComponent : IComponent<CompletedDirMappingComponent>
    {
        private readonly IWebElement _rootElement;
        private IWebElement _torrentGreaseDirElement;
        private IWebElement _torrentClientDirCsvElement;

        public List<TrackerComponent> Trackers { get; private set; }

        public CompletedDirMappingComponent(IWebElement rootElement)
        {
            _rootElement = rootElement;
        }

        public Task<CompletedDirMappingComponent> InitializeAsync()
        {
            _torrentGreaseDirElement = _rootElement.FindElementByContentName("completed-torrent-torrent-grease-dir");
            _torrentClientDirCsvElement = _rootElement.FindElementByContentName("completed-torrent-torrent-client-dir");
            return Task.FromResult(this);
        }

        public string TorrentGreaseCompletedDir
        {
            get => _torrentGreaseDirElement.Text;
            set => _torrentGreaseDirElement.SendKeys(value);
        }

        public string TorrentClientCompletedDirCsv
        {
            get => _torrentClientDirCsvElement.Text;
            set => _torrentClientDirCsvElement.SendKeys(value);
        }
    }
}
