using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Sqlite;
using System;
using Microsoft.EntityFrameworkCore;
using TorrentGrease.Data.Repositories;

namespace TorrentGrease.Data.Hosting
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTorrentGreaseData(this IServiceCollection services,
            string connectionString)
        {
            return services
                .AddTransient<TorrentGreaseDbInitializer>()
                .AddScoped<IPolicyRepository, PolicyRepository>()
                .AddScoped<ITorrentStatisticsRepository, TorrentStatisticsRepository>()
                .AddDbContext<TorrentGreaseDbContext>(o => o.UseSqlite(connectionString))
                .AddScoped<ITorrentGreaseDbContext>(s => s.GetRequiredService<TorrentGreaseDbContext>()); //Allows the dbcontext to be retrieved by interface and impl (needed for health check)
        }
    }
}
