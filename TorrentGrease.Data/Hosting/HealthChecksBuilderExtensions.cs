using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TorrentGrease.Data.Hosting
{
    public static class HealthChecksBuilderExtensions
    {
        public static IHealthChecksBuilder AddTorrentGreaseDataCheck(this IHealthChecksBuilder builder)
        {
            builder.AddDbContextCheck<TorrentGreaseDbContext>(customTestQuery: async (ctx, ct) =>
            {
                await ctx.Policies.CountAsync(ct).ConfigureAwait(false);
                return true;
            });

            return builder;
        }
    }
}
