using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TorrentGrease.Server.Blazor
{
    public static class ApplicationBuilderBlazorExtensions
    {
        public static void UseClientSideBlazorFiles(this IApplicationBuilder app, IConfiguration configuration)
        {
            var clientAssemblyPathOverride = configuration.GetValue<string>("CLIENT_APP_ASSEMBLY_PATH_OVERRIDE");
            if (!string.IsNullOrEmpty(clientAssemblyPathOverride))
            {
                app.UseClientSideBlazorFiles(clientAssemblyPathOverride);
            }
            else
            {
                app.UseClientSideBlazorFiles<Client.Startup>();
            }
        }
    }
}
