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
    public class SeleniumHooks
    {
        private const string _ScreenShotDir = "screenshots";

        [BeforeTestRun(Order = 100)]
        public static void SetupWebDriver()
        {
            //Create dir for screenshots
            Directory.CreateDirectory(_ScreenShotDir);
        }

        [AfterScenario]
        public static void SaveScenarioScreenShot(ScenarioContext scenarioContext)
        {
            var ss = ((ITakesScreenshot) DIContainer.Default.Get<IWebDriver>()).GetScreenshot();
            ss.SaveAsFile(Path.Combine(_ScreenShotDir, $"{scenarioContext.ScenarioInfo.Title}.png"),
                ScreenshotImageFormat.Png);
        }
    }
}
