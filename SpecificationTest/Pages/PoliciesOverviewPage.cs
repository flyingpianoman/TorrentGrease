using FluentAssertions;
using OpenQA.Selenium;
using SpecificationTest.Steps.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public IList<PolicyOverviewRowDto> Policies
        {
            get
            {
                var trs = _tableElement.FindElements(By.CssSelector("tbody > tr"));

                return trs
                    .Select(tr => new PolicyOverviewRowDto
                    {
                        Name = tr.FindElement(By.CssSelector("td")).Text
                    })
                    .ToList();
            }
        }
    }
}
