using OpenQA.Selenium;
using SpecificationTest.Crosscutting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SpecificationTest.Pages.Components
{
    class NavMenuLinkComponent : IComponent<NavMenuLinkComponent>
    {
        private readonly IWebElement _webElement;
        private readonly IWebDriver _webDriver;

        public NavMenuLinkComponent(IWebElement webElement, IWebDriver webDriver)
        {
            _webElement = webElement ?? throw new ArgumentNullException(nameof(webElement));
            _webDriver = webDriver?? throw new ArgumentNullException(nameof(webDriver));
        }

        public bool IsActive { get; private set; }
        public NavMenuItemTarget Target { get; private set; }

        public Task<NavMenuLinkComponent> InitializeAsync()
        {
            var href = _webElement.GetAttribute("href").Split('/').Last();

            IsActive = _webElement.GetAttribute("class").Split(' ').Any(x => x == "active");
            Target = href switch
            {
                "" => NavMenuItemTarget.Policies,
                "torrents" => NavMenuItemTarget.Torrents,
                "file-management" => NavMenuItemTarget.FileManagement,
                "file-links" => NavMenuItemTarget.FileLinks,
                _ => throw new InvalidOperationException($"Unknown nav item target href '{href}'"),
            };
            return Task.FromResult(this);
        }

        public async Task<TPage> NavigateAsync<TPage>()
            where TPage : PageBase
        {
            _webElement.Click();
            return await _webDriver.UpdateCurrentPageToAync<TPage>().ConfigureAwait(false);
        }
    }
}
