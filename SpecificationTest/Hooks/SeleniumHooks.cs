using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using SpecificationTest.Crosscutting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TechTalk.SpecFlow;

namespace SpecificationTest.Hooks
{
    [Binding]
    public static class SeleniumHooks
    {
        private const string ScreenShotDir = "screenshots";

        [BeforeTestRun(Order = 100)]
        public static void SetupWebDriver()
        {
            //Create dir for screenshots
            Directory.CreateDirectory(ScreenShotDir);
        }

        [AfterScenario]
        public static void SaveScenarioScreenShot(ScenarioContext scenarioContext)
        {
            if(scenarioContext == null) throw new ArgumentNullException(nameof(scenarioContext));

            var ss = ((ITakesScreenshot) DIContainer.Default.Get<IWebDriver>()).GetScreenshot();
            ss.SaveAsFile(Path.Combine(ScreenShotDir, $"{scenarioContext.ScenarioInfo.Title}.png"),
                ScreenshotImageFormat.Png);
        }
    }
}
