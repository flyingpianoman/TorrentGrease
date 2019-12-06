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

        public DbSet<Shared.Action> Actions { get; set; }
        public DbSet<Shared.Condition> Conditions { get; set; }
        public DbSet<Shared.Policy> Policies { get; set; }
        public DbSet<Shared.Tracker> Trackers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ConfigureActionModel(modelBuilder);
            ConfigureConditionModel(modelBuilder);
            ConfigurePolicyModel(modelBuilder);
            ConfigureTrackerModel(modelBuilder);
            ConfigureTrackerPolicyModel(modelBuilder);
        }

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
    }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
}
