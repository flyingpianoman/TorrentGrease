using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using SpecificationTest.Pages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TestUtils;

namespace SpecificationTest.Crosscutting
{
    internal static class IWebDriverExtensions
    {
        private static IPage _currentPage;

        public static void RegisterWebDriver(this DIContainer diContainer)
        {
            //    var firefoxOptions = new OpenQA.Selenium.Firefox.FirefoxOptions();
            //    firefoxOptions.AddArgument("--headless");
            //    var capabilities = firefoxOptions.ToCapabilities();


            var chromeOpts = new OpenQA.Selenium.Chrome.ChromeOptions();
            chromeOpts.AddArgument("--headless");
            chromeOpts.AddArgument("--no-sandbox");
            var capabilities = chromeOpts.ToCapabilities();
            RemoteWebDriver driver = null;

            try
            {

                TestLogger.LogElapsedTime(() =>
                {
                    driver = new RemoteWebDriver(TestSettings.SeleniumHubAddress, capabilities);
                }, "new RemoteWebDriver");
                diContainer.Register<IWebDriver>(driver);
            }
            catch (Exception)
            {
                driver?.Close();
                driver?.Dispose();
                throw;
            }
        }

        public static async Task<TPage> NavigateToPageAsync<TPage>(this IWebDriver webDriver, Uri url)
            where TPage : IPage
        {
            webDriver.Navigate().GoToUrl(url);
            var page = await CreateAndInitPageAsync<TPage>(webDriver).ConfigureAwait(false);
            _currentPage = page;
            return page;
        }

        public static async Task<TPage> UpdateCurrentPageToAync<TPage>(this IWebDriver webDriver)
            where TPage : IPage
        {
            var page = await CreateAndInitPageAsync<TPage>(webDriver).ConfigureAwait(false);
            _currentPage = page;
            return page;
        }

#pragma warning disable IDE0060 // parameter is here so that we have the extension method experience
        public static TPage CurrentPageAs<TPage>(this IWebDriver webDriver)
            where TPage : IPage
        {
            return (TPage)_currentPage;
        }
#pragma warning restore IDE0060 // parameter is here so that we have the extension method experience

        private static async Task<TPage> CreateAndInitPageAsync<TPage>(IWebDriver webDriver) where TPage : IPage
        {
            var page = (TPage)Activator.CreateInstance(typeof(TPage), new object[] { webDriver });
            await page.InitializeAsync().ConfigureAwait(false);
            return page;
        }
    }
}
