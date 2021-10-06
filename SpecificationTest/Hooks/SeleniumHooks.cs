using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using SpecificationTest.Crosscutting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;
using TestUtils;

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
            if (scenarioContext == null) throw new ArgumentNullException(nameof(scenarioContext));

            var webDriver = DIContainer.Default.Get<IWebDriver>();
            var ss = ((ITakesScreenshot)webDriver).GetScreenshot();
            ss.SaveAsFile(Path.Combine(ScreenShotDir, $"{scenarioContext.ScenarioInfo.Title}.png"),
                ScreenshotImageFormat.Png);

            var logEntries = webDriver.Manage().Logs.GetLog(LogType.Browser);
            if (logEntries.Any())
            {
                TestLogger.LogDebug("After scenario browser log entries found:");
                foreach (var logEntry in logEntries)
                {
                    TestLogger.LogDebug($"[{logEntry.Timestamp}][{logEntry.Level}] {logEntry.Message}");
                }
            }
        }
    }
}
