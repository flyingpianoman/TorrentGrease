using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using SpecificationTest.Crosscutting;

namespace SpecificationTest.Pages.Components
{
    class CheckboxComponent : IComponent<CheckboxComponent>
    {
        private readonly IWebDriver _webDriver;
        private readonly IWebElement _inputWebElement;
        private readonly IWebElement _labelWebElement;

        public string Label => _labelWebElement.Text;

        public CheckboxComponent(IWebElement webElement, IWebDriver webDriver)
        {
            _webDriver = webDriver;
            _inputWebElement = webElement;
            _labelWebElement = webElement.GetBootstrapCheckboxLabel();
        }

        public Task<CheckboxComponent> InitializeAsync()
        {
            return Task.FromResult(this);
        }

        internal void Toggle()
        {
            _inputWebElement.ClickBootstrapCheckBox(_webDriver);
        }
    }
}
