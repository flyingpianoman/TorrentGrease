using Docker.DotNet;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TorrentGrease.Data;

namespace SpecificationTest.Crosscutting
{
    public sealed class TorrentGreaseDBService : IAsyncDisposable
    {
        private static readonly string _PrepDBPath = Path.Combine(Path.GetTempPath(), "torrent-grease", "clean.db");
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
                if(_dbContext == null)
                {
                    _dbContext = LoadCleanDBAsDBContext();
                }

                return _dbContext;
            }
            set => _dbContext = value;
        }
        public async Task DownloadCleanDBFromContainerAsync()
        {
            var archiveData = await _dockerClient.Containers.GetArchiveFromContainerByNameAsync(ContainerDBPath, TestSettings.TorrentGreaseContainerName);

            using (archiveData.Stream)
            {
                _CleanDBTarMemoryStream = new MemoryStream();
                await archiveData.Stream.CopyToAsync(_CleanDBTarMemoryStream);
            }
        }

        public async Task UploadCleanDBToContainerAsync()
        {
            await _dockerClient.Containers.UploadTarredFileToContainerAsync(_CleanDBTarMemoryStream, TestSettings.TorrentGreaseContainerName, ContainerDBDirPath);
        }

        private static TorrentGreaseDbContext LoadCleanDBAsDBContext()
        {
            EnsureDirectoryIsEmpty(Path.GetDirectoryName(_PrepDBPath));
            _CleanDBTarMemoryStream.Position = 0;
            ArchiveHelper.ExtractSingleFileFromTar(_CleanDBTarMemoryStream, _PrepDBPath);

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
            await DbContext.SaveChangesAsync();
            await DbContext.DisposeAsync();
            DbContext = null;

            using var tarStream = ArchiveHelper.CreateTarStream(_PrepDBPath, ContainerDBFileName);
            await _dockerClient.Containers.UploadTarredFileToContainerAsync(tarStream, TestSettings.TorrentGreaseContainerName, ContainerDBDirPath);
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
