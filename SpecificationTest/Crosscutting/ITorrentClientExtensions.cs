using System;
using System.Collections.Generic;
using System.Text;
using TestUtils;
using TorrentGrease.TorrentClient;
using TorrentGrease.TorrentClient.Hosting;
using TorrentGrease.TorrentClient.Transmission;

namespace SpecificationTest.Crosscutting
{
    public static class ITorrentClientExtensions
    {
        private static TransmissionClient CreateTransmissionClient()
        {
            var torrentClientSettings = new TorrentClientSettings
            {
                Url = TestSettings.TransmissionExposedAddress,
                Username = TestSettings.TransmissionUser,
                Password = TestSettings.TransmissionPassword
            };

            return new TransmissionClient(TransmissionRcpClientHelper.CreateTransmissionRpcClient(torrentClientSettings));
        }

        internal static void RegisterTorrentClient(this DIContainer diContainer)
        {
            diContainer.Register<ITorrentClient>(CreateTransmissionClient());
        }
    }
}
