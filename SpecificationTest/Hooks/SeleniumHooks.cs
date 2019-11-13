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
            var capabilities = new OpenQA.Selenium.Firefox.FirefoxOptions().ToCapabilities();
            TestRunContext.WebDriver = new RemoteWebDriver(new Uri(TestSettings.SeleniumHubAddress), capabilities);

            //Create dir for screenshots
            Directory.CreateDirectory(_ScreenShotDir);
        }

        [AfterScenario]
        public static void SaveScenarioScreenShot(ScenarioContext scenarioContext)
        {
            var ss = ((ITakesScreenshot)TestRunContext.WebDriver).GetScreenshot();
            ss.SaveAsFile(Path.Combine(_ScreenShotDir, $"{scenarioContext.ScenarioInfo.Title}.png"),
                ScreenshotImageFormat.Png);
        }
    }
}
