using FluentAssertions;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SpecificationTest.Pages
{
    class PoliciesOverviewPage : PageBase
    {
        private const string _PoliciesTableId = "policies-table";
        private IWebElement _tableElement;

        public PoliciesOverviewPage(IWebDriver webDriver)
            : base(webDriver)
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            _tableElement = PageHelper.WaitForWebElementPolicy
                .Execute(() =>
                {
                    var elements = _webDriver.FindElements(By.Id(_PoliciesTableId));
                    elements.Count.Should().Be(1);

                    return elements[0];
                });
        }

        public IEnumerable<TorrentGrease.Shared.Policy> Policies
        {
            get
            {
                return new List<TorrentGrease.Shared.Policy>(); //Todo impl when we can add policies
            }
        }
    }
}
