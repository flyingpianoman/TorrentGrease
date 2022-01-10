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
        public static IWebElement GetParent(this IWebElement webElement)
        {
            return webElement.FindElement(By.XPath(".."));
        }

        public static IWebElement GetBootstrapCheckboxLabel(this IWebElement webElement)
        {
            return webElement
                .GetParent()
                .FindElement(By.CssSelector($"label[for=\"{webElement.GetAttribute("id")}\"]"));
        }

        public static void ClickBootstrapCheckBox(this IWebElement webElement, IWebDriver webDriver)
        {
            var isCheckedBeforeClick = webElement.Selected;

            //The bootstrap styling forces us to click the label instead the input
            var elementToClick = webElement.GetBootstrapCheckboxLabel();

            //We use JS because FF + Selenium have a bug that results in 'could not be scrolled into view'
            elementToClick.ClickViaJS(webDriver);

            //Wait for checkbox state to change
            PageHelper.WaitForWebElementPolicy
                .Execute(() =>
                {
                    if (webElement.Selected == isCheckedBeforeClick)
                    {
                        throw new PageHelper.RetryException();
                    }
                });
        }

        public static void ClickBootstrapRadio(this IWebElement radioElement, IWebDriver webDriver)
        {
            var radioLabelElement = radioElement.GetParent();
            var wasAlreadyActive = radioLabelElement.GetAttribute("class").Split(" ").Contains("active");

            //We use JS because FF + Selenium have a bug that results in 'could not be scrolled into view'
            radioLabelElement.ClickViaJS(webDriver);

            //Wait for radio state to change, if it wasn't active
            if(wasAlreadyActive)
            { 
                PageHelper.WaitForWebElementPolicy
                    .Execute(() =>
                    {
                        var isActive = radioLabelElement.GetAttribute("class").Split(" ").Contains("active");
                        if (!isActive)
                        {
                            throw new PageHelper.RetryException();
                        }
                    });
            }
        }

        public static void ClickViaJS(this IWebElement webElement, IWebDriver webDriver)
        {
            var jsClickCode = "arguments[0].scrollIntoView(true); arguments[0].click();";
            ((IJavaScriptExecutor)webDriver).ExecuteScript(jsClickCode, webElement);
        }

        internal static string GetContentName(this IWebElement webElement)
        {
            return webElement.GetAttribute("data-content");
        }

        public static IWebElement FindElementByContentName(this ISearchContext searchContext, string contentName)
        {
            return searchContext.FindElement(By.CssSelector($"*[data-content='{contentName}']"));
        }

        public static IReadOnlyCollection<IWebElement> FindElementsByContentName(this ISearchContext searchContext, string contentName)
        {
            return searchContext.FindElements(By.CssSelector($"*[data-content='{contentName}']"));
        }

        internal static IWebElement WaitForAnyWebElementByContentName(this ISearchContext searchContext, params string[] elementContentNames)
        {
            return PageHelper.WaitForWebElementPolicy
                .Execute(() =>
                {
                    foreach (var elementContentName in elementContentNames)
                    {
                        var elements = searchContext.FindElementsByContentName(elementContentName);
                        
                        if(elements.Any())
                        {
                            return elements.First();
                        }
                    }

                    throw new PageHelper.RetryException();
                });
        }


        internal static IWebElement WaitForWebElementByContentName(this ISearchContext searchContext, 
            string elementContentName, bool mustBeDisplayed = false)
        {
            return PageHelper.WaitForWebElementPolicy
                .Execute(() =>
                {
                    var elements = searchContext.FindElementsByContentName(elementContentName);
                    return elements.FirstOrDefault(e => !mustBeDisplayed || e.Displayed) ?? throw new PageHelper.RetryException();
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
