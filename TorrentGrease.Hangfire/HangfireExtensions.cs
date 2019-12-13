using Hangfire;
using System;
using System.Collections.Generic;
using System.Text;

namespace TorrentGrease.Hangfire
{
    public static class HangfireExtensions
    {
        public static void AddOrUpdateJob<TJob>(this IRecurringJobManager recurringJobManager, string cronExpression)
            where TJob : IJob
        {
            recurringJobManager.AddOrUpdate<TJob>(typeof(TJob).FullName, job => job.Execute(), cronExpression);
        }

        public static void AddOrUpdateAsyncJob<TJob>(this IRecurringJobManager recurringJobManager, string cronExpression)
            where TJob : IAsyncJob
        {
            recurringJobManager.AddOrUpdate<TJob>(typeof(TJob).FullName, job => job.ExecuteAsync(), cronExpression);
        }
    }
}
