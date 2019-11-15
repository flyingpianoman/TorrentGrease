using Microsoft.EntityFrameworkCore;
using SpecificationTest.Crosscutting;
using SpecificationTest.Steps.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using TorrentGrease.Data;
using TorrentGrease.Shared;

namespace SpecificationTest.Steps
{
    [Binding]
    public class DataPreperationSteps : StepsBase
    {
        private TorrentGreaseDBService TorrentGreaseDBService => this.DI.Get<TorrentGreaseDBService>();
        private TorrentGreaseDbContext DbContext => TorrentGreaseDBService.DbContext;

        [Given(@"the following trackers are staged")]
        public async Task GivenTheFollowingTrackersAreCreated(Table table)
        {
            foreach (var trackerDto in table.CreateSet<TrackerDto>())
            {
                DbContext.Trackers.Add(new Tracker
                {
                    Name = trackerDto.Name,
                    TrackerUrls = String.IsNullOrWhiteSpace(trackerDto.TrackerUrl2)
                        ? new List<string> { trackerDto.TrackerUrl1 }
                        : new List<string> { trackerDto.TrackerUrl1, trackerDto.TrackerUrl2 }
                });
            }

            await DbContext.SaveChangesAsync();
        }

        [Given(@"the following policies are staged")]
        public async Task GivenTheFollowingPoliciesAreStaged(Table table)
        {
            foreach (var policyDto in table.CreateSet<PolicyDto>())
            {
                DbContext.Policies.Add(new Policy
                {
                    Name = policyDto.Name
                });
            }

            await DbContext.SaveChangesAsync();
        }

        [Given(@"the following tracker policies are staged")]
        public async Task GivenTheFollowingTrackerPoliciesAreStaged(Table table)
        {
            foreach (var trackerPolicyDto in table.CreateSet<TrackerPolicyDto>())
            {
                var policy = DbContext.Policies
                    .Include(p => p.TrackerPolicies)
                    .Single(p => p.Name == trackerPolicyDto.Policy);
                var tracker = DbContext.Trackers.Single(p => p.Name == trackerPolicyDto.Tracker);

                policy.TrackerPolicies.Add(new TrackerPolicy
                {
                    Tracker = tracker
                });
            }

            await DbContext.SaveChangesAsync();
        }

        [Given(@"the staged data is uploaded to torrent grease")]
        public async ValueTask GivenTheStagedDataIsUploadedToTorrentGrease()
        {
            await TorrentGreaseDBService.UploadDBContextToContainerAsync();
        }

    }
}
