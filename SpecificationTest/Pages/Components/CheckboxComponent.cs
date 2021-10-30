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
        private readonly Func<Task> _onChangeAsync;

        public string Label => _labelWebElement.Text;

        public CheckboxComponent(IWebElement webElement, IWebDriver webDriver, Func<Task> onChangeAsync)
        {
            _webDriver = webDriver;
            _inputWebElement = webElement;
            _labelWebElement = webElement.GetBootstrapCheckboxLabel();
            _onChangeAsync = onChangeAsync;
        }

        public Task<CheckboxComponent> InitializeAsync()
        {
            return Task.FromResult(this);
        }

        internal async Task ToggleAsync()
        {
            _inputWebElement.ClickBootstrapCheckBox(_webDriver);
            await (_onChangeAsync?.Invoke() ?? Task.CompletedTask).ConfigureAwait(false);
        }
    }
}
