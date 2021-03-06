﻿using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SpecificationTest.Pages.Components.PolicyOverview
{
    class TrackerComponent : IComponent<TrackerComponent>
    {
        private readonly IWebElement _webElement;

        public TrackerComponent(IWebElement webElement)
        {
            this._webElement = webElement;
        }

        public string Name { get; private set; }

        public Task<TrackerComponent> InitializeAsync()
        {
            this.Name = _webElement.Text;
            return Task.FromResult(this);
        }
    }
}
