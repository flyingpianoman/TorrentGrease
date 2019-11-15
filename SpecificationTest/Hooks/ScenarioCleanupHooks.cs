using Docker.DotNet;
using Microsoft.EntityFrameworkCore;
using SpecificationTest.Crosscutting;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using TorrentGrease.Data;
using TorrentGrease.Shared;

namespace SpecificationTest.Hooks
{
    [Binding]
    public class ScenarioCleanupHooks
    {
        [BeforeTestRun(Order = 200)]
        public static async Task DownloadCleanDBFromContainerAsync()
        {
            await DIContainer.Default.Get<TorrentGreaseDBService>().DownloadCleanDBFromContainerAsync();
        }

        [BeforeScenario]
        public static async Task UploadCleanDBToContainerAsync()
        {
            await DIContainer.Default.Get<TorrentGreaseDBService>().UploadCleanDBToContainerAsync();
        }
    }
}
