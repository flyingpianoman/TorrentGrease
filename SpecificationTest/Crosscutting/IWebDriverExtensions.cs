using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using SpecificationTest.Pages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SpecificationTest.Crosscutting
{
    internal static class IWebDriverExtensions
    {
        private static IPage _currentPage;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Driver is disposed by DI container")]
        public static void RegisterWebDriver(this DIContainer diContainer)
        {
            var capabilities = new OpenQA.Selenium.Firefox.FirefoxOptions().ToCapabilities();
            var driver = new RemoteWebDriver(TestSettings.SeleniumHubAddress, capabilities);
            diContainer.Register<IWebDriver>(driver);
        }

        public static async Task<TPage> NavigateToPageAsync<TPage>(this IWebDriver webDriver, Uri url)
            where TPage : IPage
        {
            webDriver.Navigate().GoToUrl(url);
            var page = await CreateAndInitPageAsync<TPage>(webDriver).ConfigureAwait(false);
            _currentPage = page;
            return page;
        }

#pragma warning disable IDE0060 // Remove unused parameter
        public static TPage CurrentPageAs<TPage>(this IWebDriver webDriver)
            where TPage : IPage
        {
            return (TPage)_currentPage;
        }
#pragma warning restore IDE0060 // Remove unused parameter

        private static async Task<TPage> CreateAndInitPageAsync<TPage>(IWebDriver webDriver) where TPage : IPage
        {
            var page = (TPage)Activator.CreateInstance(typeof(TPage), new object[] { webDriver });
            await page.InitializeAsync().ConfigureAwait(false);
            return page;
        }
    }
}
