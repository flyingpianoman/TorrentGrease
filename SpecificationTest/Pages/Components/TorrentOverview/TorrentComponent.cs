using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using OpenQA.Selenium;

namespace SpecificationTest.Pages.Components.TorrentOverview
{
    class TorrentComponent : IComponent
    {
        private readonly IWebElement _torrentWebElement;

        public TorrentComponent(IWebElement torrentWebElement)
        {
            _torrentWebElement = torrentWebElement;
        }
        private IWebElement _isSelectedWebElement;

        public bool IsSelected
        {
            get
            {
                return _isSelectedWebElement.Selected;
            }
            set
            {
                if (IsSelected != value)
                {
                    _isSelectedWebElement.Click();
                }
            }
        }
        public string Name { get; set; }
        public string InfoHash { get; set; }
        public string Location { get; set; }
        public string JoinedTrackerUrls { get; set; }
        public decimal SizeInGB { get; set; }
        public decimal GBsOnDisk { get; set; }
        public decimal TotalUploadInGB { get; set; }
        public List<string> TrackerUrls { get; set; }


        public DateTime AddedDateTime { get; set; }

        public Task InitializeAsync()
        {
            Name = _torrentWebElement.FindElement(By.CssSelector("*[data-content='name']")).Text;
            GBsOnDisk = Decimal.Parse(_torrentWebElement.FindElement(By.CssSelector("*[data-content='data-on-disk-in-gb']")).Text, CultureInfo.InvariantCulture);
            SizeInGB = Decimal.Parse(_torrentWebElement.FindElement(By.CssSelector("*[data-content='size-in-gb']")).Text, CultureInfo.InvariantCulture);
            TotalUploadInGB = Decimal.Parse(_torrentWebElement.FindElement(By.CssSelector("*[data-content='total-upload-in-gb']")).Text, CultureInfo.InvariantCulture);
            AddedDateTime = DateTime.Parse(_torrentWebElement.FindElement(By.CssSelector("*[data-content='date-added']")).Text, CultureInfo.InvariantCulture);
            Location = _torrentWebElement.FindElement(By.CssSelector("*[data-content='location']")).Text;
            JoinedTrackerUrls = _torrentWebElement.FindElement(By.CssSelector("*[data-content='trackerUrls']")).Text;
            TrackerUrls = JoinedTrackerUrls.Split(", ").ToList();
            _isSelectedWebElement = _torrentWebElement.FindElement(By.CssSelector("*[data-content='selector']"));

            return Task.CompletedTask;
        }
    }
}
