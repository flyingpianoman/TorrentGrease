using OpenQA.Selenium;
using SpecificationTest.Pages.Components;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SpecificationTest.Pages
{
    abstract class PageBase : IPage
    {
        protected readonly IWebDriver _webDriver;
        public NavMenuComponent NavigationMenu { get; private set; }

        public PageBase(IWebDriver webDriver)
        {
            _webDriver = webDriver ?? throw new ArgumentNullException(nameof(webDriver));
        }

        public virtual async Task InitializeAsync()
        {
            NavigationMenu = new NavMenuComponent(_webDriver);
            await NavigationMenu.InitializeAsync().ConfigureAwait(false);
        }
    }
}
