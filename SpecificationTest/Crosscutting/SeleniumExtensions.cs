using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQA.Selenium;
using SpecificationTest.Pages;

namespace SpecificationTest.Crosscutting
{
    static class SeleniumExtensions
    {
        public static void ClickBootstrapCheckBox(this IWebElement webElement, IWebDriver webDriver)
        {
            //The bootstrap styling forces us to click the label instead the input
            var elementToClick = webElement
                .FindElement(By.XPath("..")) //parent element
                .FindElement(By.CssSelector($"label[for=\"{webElement.GetAttribute("id")}\"]"));

            //We use JS because FF + Selenium have a bug that results in 'could not be scrolled into view'
            elementToClick.ClickViaJS(webDriver);
        }

        public static void ClickViaJS(this IWebElement webElement, IWebDriver webDriver)
        {
            var jsClickCode = "arguments[0].scrollIntoView(true); arguments[0].click();";
            ((IJavaScriptExecutor)webDriver).ExecuteScript(jsClickCode, webElement);
        }

        public static IWebElement FindElementByContentName(this ISearchContext searchContext, string contentName)
        {
            return searchContext.FindElement(By.CssSelector($"*[data-content='{contentName}']"));
        }

        public static IReadOnlyCollection<IWebElement> FindElementsByContentName(this ISearchContext searchContext, string contentName)
        {
            return searchContext.FindElements(By.CssSelector($"*[data-content='{contentName}']"));
        }

        internal static IWebElement WaitForWebElementByContentName(this ISearchContext searchContext, string elementContentName)
        {
            return PageHelper.WaitForWebElementPolicy
                .Execute(() =>
                {
                    var elements = searchContext.FindElementsByContentName(elementContentName);
                    return elements.FirstOrDefault() ?? throw new PageHelper.RetryException();
                });
        }

        internal static void WaitForWebElementToCloseByContentName(this ISearchContext searchContext, string elementContentName)
        {
            PageHelper.WaitForWebElementPolicy
                .Execute(() =>
                {
                    var elements = searchContext.FindElementsByContentName(elementContentName);
                    if (elements.Any(e => e.Displayed))
                    {
                        throw new PageHelper.RetryException();
                    }
                });
        }

    }
}
