using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using TorrentGrease.Shared;

namespace TorrentGrease.Data
{
    public interface ITorrentGreaseDbContext
    {
        DatabaseFacade Database { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        ValueTask<EntityEntry<TEntity>> AddAsync<TEntity>([NotNullAttribute] TEntity entity, CancellationToken cancellationToken = default) where TEntity : class;
        Task AddRangeAsync([NotNullAttribute] IEnumerable<object> entities, CancellationToken cancellationToken = default);

        //Policy schema
        DbSet<Action> Actions { get; set; }
        DbSet<Condition> Conditions { get; set; }
        DbSet<Policy> Policies { get; set; }
        DbSet<Tracker> Trackers { get; set; }

        //Statistics schema
        DbSet<Shared.TorrentStatistics.Torrent> Torrents { get; set; }
        DbSet<Shared.TorrentStatistics.TorrentUploadDeltaSnapshot> TorrentUploadDeltaSnapshots { get; set; }
        DbSet<Shared.TorrentStatistics.TrackerUrl> TrackerUrls { get; set; }
        DbSet<Shared.TorrentStatistics.TrackerUrlCollection> TrackerUrlCollections { get; set; }
    }
}