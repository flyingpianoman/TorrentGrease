using Docker.DotNet;
using SpecificationTest.Crosscutting;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using TestUtils;
using TestUtils.Torrent;

namespace SpecificationTest.Hooks
{
    [Binding]
    public sealed class DependencyInjectionHooks
    {
        [BeforeTestRun(Order = 1)]
        public static void InitDIContainer()
        {
            TestLogger.LogElapsedTime(() =>
            {
                var services = DIContainer.Default;
                services.RegisterGrpcClients();
                services.RegisterDockerClient();
                services.RegisterTorrentClient();
                services.RegisterWebDriver();
                services.Register(new TorrentGreaseDBService(services.Get<DockerClient>()));
                services.Register(new TorrentFileHelper());
                services.Register(new Dictionary<string, string>(), "TorrentDataFolders");
            }, nameof(InitDIContainer));
        }

        [AfterTestRun]
        public static async ValueTask DisposeDIContainer()
        {
            await TestLogger.LogElapsedTimeAsync(async () =>
            {
                await DIContainer.Default.DisposeAsync();
            }, nameof(DisposeDIContainer));
        }
    }
}
