using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorrentGrease.Data.Repositories
{
    public class PolicyRepository : IPolicyRepository
    {
        private readonly ITorrentGreaseDbContext _dbContext;

        public PolicyRepository(ITorrentGreaseDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<ICollection<Shared.Policy>> GetAllAsync()
        {
            var policies = await _dbContext
                .Policies
                .Include(p => p.TrackerPolicies)
                    .ThenInclude((Shared.TrackerPolicy tp) => tp.Tracker)
                .ToListAsync().ConfigureAwait(false);

            foreach (var trackerPolicy in policies.SelectMany(p => p.TrackerPolicies))
            {
                //Circular references aren't supported in this version of protobuf-net.core yet so null some navigation properties
                trackerPolicy.Policy = null; 
                trackerPolicy.Tracker.TrackerPolicies = null;
            }

            return policies;
        }
    }
}
