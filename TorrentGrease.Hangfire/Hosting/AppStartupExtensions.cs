using Hangfire;
using Hangfire.Dashboard;
using Hangfire.Storage.SQLite;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using TorrentGrease.Hangfire;

namespace TorrentGrease.Hangfire.Hosting
{
    public static class AppStartupExtensions
    {
        public static IServiceCollection AddHangfire(this IServiceCollection services, IConfiguration configuration)
        {
            var sqliteStorageOptions = new SQLiteStorageOptions
            {
                InvisibilityTimeout = TimeSpan.FromMinutes(5)
            };

            return services
                .AddHangfire(c => c
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UseSQLiteStorage(configuration.GetConnectionString("HangfireConnection"), sqliteStorageOptions))
                .AddHangfireServer(o => o.WorkerCount = 8);
        }

        public static IApplicationBuilder UseHangfire(this IApplicationBuilder app)
        {
            return app
                .UseHangfireDashboard(options: new DashboardOptions { Authorization = new List<IDashboardAuthorizationFilter> { new AnonymousAuthFilter() } });
        }
    }
}
