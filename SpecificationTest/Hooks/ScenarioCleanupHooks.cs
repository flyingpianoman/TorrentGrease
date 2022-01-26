using Docker.DotNet;
using Microsoft.EntityFrameworkCore;
using SpecificationTest.Crosscutting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using TestUtils;
using TorrentGrease.Data;
using TorrentGrease.Shared;

namespace SpecificationTest.Hooks
{
    [Binding]
    public static class ScenarioCleanupHooks
    {
        [BeforeTestRun(Order = 200)]
        public static async Task CreateCleanDBAsync()
        {
            await TestLogger.LogElapsedTimeAsync(() => TorrentGreaseDBService.CreateCleanDBAsync(), nameof(CreateCleanDBAsync)).ConfigureAwait(false);
        }

        [BeforeScenario]
        public static async Task UploadCleanDBToContainerAsync()
        {
            await TestLogger.LogElapsedTimeAsync(async () =>
            {
                DIContainer.Default.Get<Dictionary<string, string>>("TorrentDataFolders").Clear();

                var torrentGreaseDBService = DIContainer.Default.Get<TorrentGreaseDBService>();
                await torrentGreaseDBService.UploadCleanDBToContainerAsync().ConfigureAwait(false);
                torrentGreaseDBService.DbContext = null; //ensures that a clean prep dbcontext is created each test
            }, nameof(UploadCleanDBToContainerAsync));
        }
    }
}
