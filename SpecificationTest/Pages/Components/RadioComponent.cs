using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using SpecificationTest.Crosscutting;

namespace SpecificationTest.Pages.Components
{
    class RadioComponent : IComponent<RadioComponent>
    {
        private readonly IWebElement _rootElement;
        private readonly IWebDriver _webDriver;

        public RadioComponent(IWebElement element, IWebDriver webDriver)
        {
            _rootElement = element;
            _webDriver = webDriver;
        }

        public Task<RadioComponent> InitializeAsync()
        {
            return Task.FromResult(this);
        }

        public IEnumerable<string> Values => GetRadios()
                        .Select(el => el.GetAttribute("value"))
                        .ToArray();

        private IEnumerable<IWebElement> GetRadios()
        {
            return _rootElement
                        .FindElements(By.CssSelector("label > input[type='radio']"));
        }

        public string ActiveValue 
        {
            get => _rootElement
                        .FindElement(By.CssSelector("label.active > input[type='radio']"))
                        ?.GetAttribute("value");
        }

        public void ClickRadioOptionByValue(string value)
        {
            GetRadios()
                .First(e => e.GetAttribute("value") == value)
                .ClickBootstrapRadio(_webDriver);
        }
    }
}
