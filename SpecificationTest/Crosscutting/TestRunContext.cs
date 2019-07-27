using OpenQA.Selenium;
using SpecificationTest.Pages;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpecificationTest.Crosscutting
{
    internal static class TestRunContext
    {
        public static IWebDriver WebDriver { get; set; }
        public static IPage CurrentPage { get; set; }
    }
}
