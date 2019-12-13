using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace TorrentGrease.Data.Hosting
{
    public class TorrentGreaseDbInitializer
    {
        private readonly ITorrentGreaseDbContext _torrentGreaseDbContext;

        public TorrentGreaseDbInitializer(ITorrentGreaseDbContext torrentGreaseDbContext)
        {
            _torrentGreaseDbContext = torrentGreaseDbContext ?? throw new ArgumentNullException(nameof(torrentGreaseDbContext));
        }

        /// <summary>
        /// Ensures that the DB is created and migrated to the latest version
        /// </summary>
        public async Task InitializeAsync()
        {
            EnsureDbDirExists();
            await _torrentGreaseDbContext.Database.MigrateAsync().ConfigureAwait(false);
        }

        private void EnsureDbDirExists()
        {
            var con = (SqliteConnection)_torrentGreaseDbContext.Database.GetDbConnection();
            var dirContainingDbFile = Path.GetDirectoryName(con.DataSource);

            if (!Directory.Exists(dirContainingDbFile))
            {
                Directory.CreateDirectory(dirContainingDbFile);
            }
        }
    }
}
