using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SpecificationTest.Crosscutting;

namespace SpecificationTest.Pages.Components.TorrentOverview
{
    class TorrentRelocationCandidateComponent : IComponent<TorrentRelocationCandidateComponent>
    {
        private readonly IWebElement _rootElement;
        private readonly IWebDriver _webDriver;
        private IWebElement _isSelectedWebElement;
        public bool IsSelected 
        {
            get => _isSelectedWebElement.Selected;
            set
            {
                if(IsSelected != value)
                {
                    _isSelectedWebElement.ClickBootstrapCheckBox(_webDriver);
                }
            }
        }

        public bool IsSelectable => RelocateOptionsCount > 0;

        public string TorrentName { get; private set; }
        public int RelocateOptionsCount { get; private set; }

        private IWebElement _relocateOptionsSelectorElement;

        public string[] RelocateOptions { get; private set; }
        public string SelectedRelocateOption
        {
            get => new SelectElement(_relocateOptionsSelectorElement).SelectedOption.Text;
            set => new SelectElement(_relocateOptionsSelectorElement).SelectByValue(value);
        }

        public TorrentRelocationCandidateComponent(IWebElement rootElement, IWebDriver webDriver)
        {
            this._rootElement = rootElement;
            _webDriver = webDriver;
        }

        public Task<TorrentRelocationCandidateComponent> InitializeAsync()
        {
            TorrentName = _rootElement.FindElementByContentName("torrent-name").Text;
            RelocateOptionsCount = int.Parse(_rootElement.FindElementByContentName("relocate-options-count").Text);
            SetSelectorWebElement();
            SetRelocateOptions();

            return Task.FromResult(this);
        }

        private void SetRelocateOptions()
        {
            var relocateOptionsSelectorEl = _rootElement.WaitForAnyWebElementByContentName("relocate-options", "no-relocate-options");
            _relocateOptionsSelectorElement = relocateOptionsSelectorEl.GetContentName() == "relocate-options"
                ? relocateOptionsSelectorEl
                : null;

            RelocateOptions = _relocateOptionsSelectorElement == null
                ? Array.Empty<string>()
                : _relocateOptionsSelectorElement.FindElementsByContentName("relocate-option")
                    .Select(e => e.Text)
                    .ToArray();
        }

        private void SetSelectorWebElement()
        {
            var selectorOrNoSelectorEl = _rootElement.WaitForAnyWebElementByContentName("selector", "no-selector");
            _isSelectedWebElement = selectorOrNoSelectorEl.GetContentName() == "selector"
                ? selectorOrNoSelectorEl
                : null;
        }
    }
}
