﻿using Docker.DotNet;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TestUtils;
using TorrentGrease.Data;
using TorrentGrease.Data.Hosting;

namespace SpecificationTest.Crosscutting
{
    public sealed class TorrentGreaseDBService : IAsyncDisposable
    {
        private static readonly string _PrepDBPath = Path.Combine(Path.GetTempPath(), "torrent-grease", "prep.db");
        public const string ContainerDBDirPath = "/app/data";
        public const string ContainerDBFileName = "TorrentGrease.db";
        public const string ContainerDBPath = ContainerDBDirPath + "/" + ContainerDBFileName;

        private readonly DockerClient _dockerClient;
        private static MemoryStream _CleanDBTarMemoryStream;
        private TorrentGreaseDbContext _dbContext;

        public TorrentGreaseDBService(DockerClient dockerClient)
        {
            _dockerClient = dockerClient;
        }

        public TorrentGreaseDbContext DbContext
        {
            get
            {
                if (_dbContext == null)
                {
                    _dbContext = LoadCleanDBAsDBContext();
                }

                return _dbContext;
            }
            set => _dbContext = value;
        }
        public static async Task CreateCleanDBAsync()
        {
            if (File.Exists(_PrepDBPath))
            {
                File.Delete(_PrepDBPath);
            }

            using var dbContext = CreateDBContext();
            try
            {
                var dbInitializer = new TorrentGreaseDbInitializer(dbContext);
                await dbInitializer.InitializeAsync().ConfigureAwait(false);
            }
            finally
            {
                await dbContext.DisposeAsync(); //make sure that everything is written to disk
            }

            _CleanDBTarMemoryStream = ArchiveHelper.CreateSingleFileTarStream(_PrepDBPath, ContainerDBFileName);
        }

        public async Task UploadCleanDBToContainerAsync()
        {
            await _dockerClient.Containers.UploadTarredFileToContainerAsync(_CleanDBTarMemoryStream, TestSettings.TorrentGreaseContainerName, ContainerDBDirPath).ConfigureAwait(false);
        }

        private static TorrentGreaseDbContext LoadCleanDBAsDBContext()
        {
            EnsureDirectoryIsEmpty(Path.GetDirectoryName(_PrepDBPath));
            _CleanDBTarMemoryStream.Position = 0;
            ArchiveHelper.ExtractSingleFileFromTar(_CleanDBTarMemoryStream, _PrepDBPath);

            return CreateDBContext();
        }

        private static TorrentGreaseDbContext CreateDBContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<TorrentGreaseDbContext>();
            optionsBuilder.UseSqlite("Data Source=" + _PrepDBPath);
            return new TorrentGreaseDbContext(optionsBuilder.Options);
        }

        private static void EnsureDirectoryIsEmpty(string dir)
        {
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, recursive: true);
            }
            Directory.CreateDirectory(dir);
        }

        public async Task UploadDBContextToContainerAsync()
        {
            await DbContext.SaveChangesAsync().ConfigureAwait(false);
            await DbContext.DisposeAsync();

            using var tarStream = ArchiveHelper.CreateSingleFileTarStream(_PrepDBPath, ContainerDBFileName);
            await _dockerClient.Containers.UploadTarredFileToContainerAsync(tarStream, TestSettings.TorrentGreaseContainerName, ContainerDBDirPath).ConfigureAwait(false);

            DbContext = CreateDBContext();
        }

        public async ValueTask DisposeAsync()
        {
            await _CleanDBTarMemoryStream.DisposeAsync();
            if (_dbContext != null)
            {
                await _dbContext.DisposeAsync();
            }
        }
    }
}
