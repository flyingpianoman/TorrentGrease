﻿using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium.Remote;
using Polly;
using Polly.Wrap;
using SpecificationTest.Crosscutting;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using TestUtils;
using TorrentGrease.TorrentClient;
using TorrentGrease.TorrentClient.Hosting;
using TorrentGrease.TorrentClient.Transmission;

namespace SpecificationTest.Hooks
{
    [Binding]
    public static class WaitForServicesHooks
    {
        private static readonly AsyncPolicyWrap _WaitForHealthyPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<AssertFailedException>()
            .WaitAndRetryUntilTimeoutAsync(
                retryDelay: TimeSpan.FromMilliseconds(100),
                timeout: TimeSpan.FromSeconds(10));

        [BeforeTestRun(Order = 2)]
        public static async Task WaitForServices()
        {
            await Task.WhenAll(WaitForTransmissionAsync(),
                               WaitForTorrentGreaseAsync(),
                               WaitForSeleniumHubAsync()).ConfigureAwait(false);
        }

        private static async Task WaitForTransmissionAsync()
        {
            var torrentClient = DIContainer.Default.Get<ITorrentClient>();

            try
            {
                await _WaitForHealthyPolicy.ExecuteAsync(async () =>
                    {
                        await torrentClient.GetAllTorrentsAsync().ConfigureAwait(false);
                    }).ConfigureAwait(false);
            }
            catch (Polly.Timeout.TimeoutRejectedException e)
            {
                throw new ApplicationException("Timed out while waiting on the torrent client", e);
            }
        }

        private static async Task WaitForTorrentGreaseAsync()
        {

            await TestLogger.LogElapsedTimeAsync(async () =>
            {
                using var httpClient = new HttpClient();
                try
                {
                    await _WaitForHealthyPolicy.ExecuteAsync(async () =>
                        {
                            var healthUri = new Uri(TestSettings.TorrentGreaseExposedAddress + "/health");
                            using var resp = await httpClient.GetAsync(healthUri).ConfigureAwait(false);
                            resp.StatusCode.Should().Be(HttpStatusCode.OK);
                        }).ConfigureAwait(false);
                }
                catch (Polly.Timeout.TimeoutRejectedException e)
                {
                    throw new ApplicationException("Timed out while waiting on TorrentGrease", e);
                }
            }, nameof(WaitForTorrentGreaseAsync));
        }

        private static async Task WaitForSeleniumHubAsync()
        {

            await TestLogger.LogElapsedTimeAsync(async () =>
            {
                using var httpClient = new HttpClient();
                try
                {
                    await _WaitForHealthyPolicy.ExecuteAsync(async () =>
                        {
                            var healthUri = new Uri(TestSettings.SeleniumHubAddress + "wd/hub/status");
                            using var resp = await httpClient.GetAsync(healthUri).ConfigureAwait(false);
                            resp.StatusCode.Should().Be(HttpStatusCode.OK);
                            var statusJson = JObject.Parse(await resp.Content.ReadAsStringAsync().ConfigureAwait(false));
                            statusJson["value"]["ready"].Value<bool>().Should().BeTrue();
                        }).ConfigureAwait(false);
                }
                catch (Polly.Timeout.TimeoutRejectedException e)
                {
                    throw new ApplicationException("Timed out while waiting on SeleniumHub", e);
                }
            }, nameof(WaitForSeleniumHubAsync));
        }
    }
}
