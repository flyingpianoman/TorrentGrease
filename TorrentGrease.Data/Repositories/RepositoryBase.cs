using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TorrentGrease.Data.Repositories
{
    public abstract class RepositoryBase : IRepository
    {
        protected readonly ITorrentGreaseDbContext _dbContext;

        public RepositoryBase(ITorrentGreaseDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _dbContext.SaveChangesAsync(cancellationToken);
        }

        public Task AddRangeAsync([NotNullAttribute] IEnumerable<object> entities, CancellationToken cancellationToken = default)
        {
            return _dbContext.AddRangeAsync(entities, cancellationToken);
        }

        public async ValueTask AddAsync<TEntity>([NotNull] TEntity entity, CancellationToken cancellationToken = default) where TEntity : class
        {
            await _dbContext.AddAsync(entity, cancellationToken);
        }
    }
}
