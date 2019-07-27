using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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
            return await _dbContext
                .Policies
                .ToListAsync().ConfigureAwait(false);
        }
    }
}
