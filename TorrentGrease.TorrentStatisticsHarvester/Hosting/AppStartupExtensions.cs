using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using TorrentGrease.Hangfire;

namespace TorrentGrease.TorrentStatisticsHarvester.Hosting
{
    public static class AppStartupExtensions
    {
        public static IServiceCollection AddTorrentStatisticsHarvester(this IServiceCollection services)
        {
            return services
                .AddScoped<TorrentStatisticsHarvesterJob>();
        }

        public static IApplicationBuilder UseTorrentStatisticsHarvester(this IApplicationBuilder app,
            IServiceProvider serviceProvider)
        {
            var recuringJobManager = serviceProvider.GetRequiredService<IRecurringJobManager>();
            recuringJobManager.AddOrUpdateJob<TorrentStatisticsHarvesterJob>("*/5 * * * *"); //every 5 min

            return app;
        }
    }
}
