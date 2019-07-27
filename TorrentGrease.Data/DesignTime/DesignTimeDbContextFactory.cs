using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TorrentGrease.Data.DesignTime
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<TorrentGreaseDbContext>
    {
        public TorrentGreaseDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<TorrentGreaseDbContext>();
            builder.UseSqlite("Filename=./DesignTime/TorrentGreaseDesign.db");
            return new TorrentGreaseDbContext(builder.Options);
        }
    }
}
