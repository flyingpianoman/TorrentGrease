using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TorrentGrease.Data.Repositories
{
    public interface IRepository
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        ValueTask AddAsync<TEntity>([NotNullAttribute] TEntity entity, CancellationToken cancellationToken = default) where TEntity : class;
        Task AddRangeAsync([NotNullAttribute] IEnumerable<object> entities, CancellationToken cancellationToken = default);
    }
}
