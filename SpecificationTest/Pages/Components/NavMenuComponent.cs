using FluentAssertions;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecificationTest.Pages.Components
{
    class NavMenuComponent : IComponent
    {
        private const string _NavMenuLinkSelector = "#navMenu > ul.nav > li > a";
        private readonly IWebDriver _webDriver;
        public List<NavMenuLinkComponent> NavMenuLinks { get; private set; }

        public NavMenuComponent(IWebDriver webDriver)
        {
            _webDriver = webDriver;
        }


        public async Task InitializeAsync()
        {
            NavMenuLinks = await PageHelper.WaitForWebElementPolicyAsync
                .ExecuteAsync(async () =>
                {
                    var elements = _webDriver.FindElements(By.CssSelector(_NavMenuLinkSelector));
                    elements.Should().NotBeEmpty();
                    var links = new List<NavMenuLinkComponent>(elements.Count);

                    foreach (var element in elements)
                    {
                        var link = new NavMenuLinkComponent(element);
                        await link.InitializeAsync();
                        links.Add(link);
                    }

                    return links;
                });
        }
    }
}
