using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using SpecificationTest.Steps.Models;
using SpecificationTest.Steps.State;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using TorrentGrease.Shared.ServiceContracts;

namespace SpecificationTest.Steps
{
    [Binding]
    public class TrackerUrlCollectionsSteps : StepsBase
    {
        private readonly ITorrentService _torrentService = DI.Get<ITorrentService>();

        [When(@"I get the current tracker url collections")]
        public void WhenIGetTheCurrentTrackerUrlCollections()
        {
            InnerAsync().GetAwaiter().GetResult();

            async Task InnerAsync()
            {
                DI.GetState<CurrentTrackerUrlCollectionsState>().LastRetrievedTrackerUrlCollection =
                    await _torrentService.GetCurrentTrackerUrlCollectionsAsync();
            }
        }

        [Then(@"I get (\d+) current tracker url collections")]
        public void ThenIGetCurrentTrackerUrlCollections(int nr)
        {
            DI.GetState<CurrentTrackerUrlCollectionsState>().LastRetrievedTrackerUrlCollection
                .Should().HaveCount(nr);
        }

        [Then(@"I get the following current tracker url collections")]
        public void ThenIGetTheFollowingCurrentTrackerUrlCollections(Table table)
        {
            var expectedCols = table.CreateSet<TrackerUrlCollectionDto>().ToList();
            var actualCols = DI.GetState<CurrentTrackerUrlCollectionsState>().LastRetrievedTrackerUrlCollection;

            actualCols.Should().HaveCount(expectedCols.Count);
            foreach (var expectedCol in expectedCols)
            {
                var actualCol = actualCols.First(c => c.TrackerUrls.First() == expectedCol.TrackerAnnounceUrl1);
                actualCol.TrackerUrls.Should().BeEquivalentTo(expectedCol.TrackerAnnounceUrls);
            }
        }
    }
}
