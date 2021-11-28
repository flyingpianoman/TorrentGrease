using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Docker.DotNet;
using SpecificationTest.Crosscutting;
using TechTalk.SpecFlow;
using TestUtils;
using TorrentGrease.TorrentClient;

namespace SpecificationTest.Hooks
{
    [Binding]
    public static class TorrentClientCleanupHook
    {
        [BeforeScenario]
        public static async Task ResetTransmissionAsync()
        {
            var torrentClient = DIContainer.Default.Get<ITorrentClient>();
            var torrentIDs = (await torrentClient.GetAllTorrentsAsync().ConfigureAwait(false))
                .Select(t => t.ID)
                .ToArray();

            await torrentClient.RemoveTorrentsByIDsAsync(torrentIDs, deleteData: true).ConfigureAwait(false);

            var dockerClient = DIContainer.Default.Get<DockerClient>();
            var transmissionContainerId = await dockerClient.Containers.GetContainerIdByNameAsync(TestSettings.TransmissionContainerName).ConfigureAwait(false);
            await dockerClient.EmptyDirInContainerAsync(transmissionContainerId, "/downloads/complete").ConfigureAwait(false);
            await dockerClient.EmptyDirInContainerAsync(transmissionContainerId, "/downloads/incomplete").ConfigureAwait(false);
            await dockerClient.EmptyDirInContainerAsync(transmissionContainerId, "/downloads/unmapped").ConfigureAwait(false);
            await dockerClient.EmptyDirInContainerAsync(transmissionContainerId, "/second-downloads").ConfigureAwait(false);


        }
    }
}
