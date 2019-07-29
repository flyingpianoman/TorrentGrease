using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecificationTest.Pages.Components
{
    class NavMenuLinkComponent : IComponent
    {
        private readonly IWebElement _webElement;

        public NavMenuLinkComponent(IWebElement webElement)
        {
            _webElement = webElement ?? throw new ArgumentNullException(nameof(webElement));
        }

        public NavMenuItemTarget Target { get; private set; }

        public Task InitializeAsync()
        {
            var href = _webElement.GetAttribute("href").Split('/').Last();

            switch (href)
            {
                case "":
                case "torrents":
                    Target = NavMenuItemTarget.Policies;
                    break;
                default:
                    throw new InvalidOperationException($"Unknown nav item target href '{href}'");
            }

            return Task.CompletedTask;
        }
    }
}
