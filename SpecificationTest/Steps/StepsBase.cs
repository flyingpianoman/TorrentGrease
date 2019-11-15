using OpenQA.Selenium;
using SpecificationTest.Crosscutting;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpecificationTest.Steps
{
    public abstract class StepsBase
    {
        public DIContainer DI => DIContainer.Default;
        protected IWebDriver WebDriver => this.DI.Get<IWebDriver>();
    }
}
