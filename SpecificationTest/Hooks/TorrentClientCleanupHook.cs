using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        }
    }
}
