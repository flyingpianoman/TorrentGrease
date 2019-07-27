using OpenQA.Selenium;
using SpecificationTest.Pages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SpecificationTest.Crosscutting
{
    internal static class IWebDriverExtensions
    {
        public static async Task<TPage> NavigateToPageAsync<TPage>(this IWebDriver webDriver, string url)
            where TPage : IPage
        {
            webDriver.Navigate().GoToUrl(url);
            var page = await CreateAndInitPageAsync<TPage>(webDriver).ConfigureAwait(false);
            TestRunContext.CurrentPage = page;
            return page;
        }

#pragma warning disable IDE0060 // Remove unused parameter
        public static TPage CurrentPageAs<TPage>(this IWebDriver webDriver)
            where TPage : IPage
        {
            return (TPage) TestRunContext.CurrentPage;
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
