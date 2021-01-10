using OpenQA.Selenium;
using SpecificationTest.Crosscutting;
using System;
using System.Collections.Generic;
using System.Text;
using TestUtils;

namespace SpecificationTest.Steps
{
    public abstract class StepsBase
    {
        public static DIContainer DI => DIContainer.Default;
        protected IWebDriver WebDriver => DI.Get<IWebDriver>();
    }
}
