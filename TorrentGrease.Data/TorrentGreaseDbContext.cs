using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TorrentGrease.Data
{
#pragma warning disable CS8618 // Non-nullable field is uninitialized, are set by EF
    public class TorrentGreaseDbContext : DbContext, ITorrentGreaseDbContext
    {
        public TorrentGreaseDbContext(DbContextOptions<TorrentGreaseDbContext> options)
            : base(options)
        {
        }

        //Policy schema
        public DbSet<Shared.Action> Actions { get; set; }
        public DbSet<Shared.Condition> Conditions { get; set; }
        public DbSet<Shared.Policy> Policies { get; set; }
        public DbSet<Shared.Tracker> Trackers { get; set; }

        //Statistics schema
        public DbSet<Shared.TorrentStatistics.Torrent> Torrents { get; set; }
        public DbSet<Shared.TorrentStatistics.TorrentUploadDeltaSnapshot> TorrentUploadDeltaSnapshots { get; set; }
        public DbSet<Shared.TorrentStatistics.TrackerUrl> TrackerUrls { get; set; }
        public DbSet<Shared.TorrentStatistics.TrackerUrlCollection> TrackerUrlCollections { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Policy schema
            ConfigureActionModel(modelBuilder);
            ConfigureConditionModel(modelBuilder);
            ConfigurePolicyModel(modelBuilder);
            ConfigureTrackerModel(modelBuilder);
            ConfigureTrackerPolicyModel(modelBuilder);

            //Statistics schema
            ConfigureTorrentModel(modelBuilder);
            ConfigureTorrentUploadDeltaSnapshotModel(modelBuilder);
            ConfigureTrackerUrlCollectionModel(modelBuilder);
            ConfigureTrackerUrlModel(modelBuilder);
        }

        #region Statistics schema
        private static void ConfigureTorrentModel(ModelBuilder modelBuilder)
        {
            var b = modelBuilder
                .Entity<Shared.TorrentStatistics.Torrent>()
                .ToTable(nameof(Shared.TorrentStatistics.Torrent));

            b.HasIndex(t => t.InfoHash);
            b.HasIndex(t => t.WasInClientOnLastScan);
        }

        private static void ConfigureTorrentUploadDeltaSnapshotModel(ModelBuilder modelBuilder)
        {
            var b = modelBuilder
                .Entity<Shared.TorrentStatistics.TorrentUploadDeltaSnapshot>()
                .ToTable(nameof(Shared.TorrentStatistics.TorrentUploadDeltaSnapshot));

            b.HasIndex(t => new { t.TorrentId, t.DateTime });
        }

        private static void ConfigureTrackerUrlCollectionModel(ModelBuilder modelBuilder)
        {
            var b = modelBuilder
                .Entity<Shared.TorrentStatistics.TrackerUrlCollection>()
                .ToTable(nameof(Shared.TorrentStatistics.TrackerUrlCollection));
        }

        private static void ConfigureTrackerUrlModel(ModelBuilder modelBuilder)
        {
            var b = modelBuilder
                .Entity<Shared.TorrentStatistics.TrackerUrl>()
                .ToTable(nameof(Shared.TorrentStatistics.TrackerUrl));

            b.HasIndex(t => t.TrackerUrlCollectionId);
        }
        #endregion

        #region Policy schema

        private static void ConfigureTrackerPolicyModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Shared.TrackerPolicy>()
                .ToTable(nameof(Shared.TrackerPolicy))
                .HasKey(e => new { e.TrackerId, e.PolicyId });
        }

        private static void ConfigureTrackerModel(ModelBuilder modelBuilder)
        {
            var trackerBuilder = modelBuilder
                .Entity<Shared.Tracker>()
                .ToTable(nameof(Shared.Tracker));

        }

        private static void ConfigurePolicyModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Shared.Policy>()
                .Ignore(p => p.Trackers)
                .ToTable(nameof(Shared.Policy));
        }

        private static void ConfigureConditionModel(ModelBuilder modelBuilder)
        {
            var b = modelBuilder
                .Entity<Shared.Condition>()
                .ToTable(nameof(Shared.Condition));

            b.HasIndex(e => e.Order)
                .IsUnique();

            b.Property(e => e.ConditionType)
                .HasConversion(new EnumToStringConverter<Shared.ConditionType>());
        }

        private static void ConfigureActionModel(ModelBuilder modelBuilder)
        {
            var actionBuilder = modelBuilder
                .Entity<Shared.Action>()
                .ToTable(nameof(Shared.Action));

            actionBuilder.HasIndex(e => e.Order)
                .IsUnique();

            actionBuilder.Property(e => e.ActionType)
                .HasConversion(new EnumToStringConverter<Shared.ActionType>());
        } 
        #endregion
    }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
}
