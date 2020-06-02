using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestUtils.Torrent;

namespace IntegrationTest.Torrent
{
    [TestClass]
    public class TorrentFileHelperTest
    {
        private const string TestFilesDir = "Torrent/TestFiles";
        private List<string> _createdTmpFiles;

        [TestInitialize]
        public void TestInit()
        {
            _createdTmpFiles = new List<string>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            foreach (var tmpFile in _createdTmpFiles)
            {
                if (File.Exists(tmpFile))
                {
                    File.Delete(tmpFile);
                }
            }
        }

        #region CreateTextFileAsync
        [TestMethod]
        public async Task CreateTextFileAsync_10B_FileOf10BSize()
        {
            //Arrange
            await CreateTextFileAsyncTestBodyAsync(fileSizeInBytes: 10);
        }

        [TestMethod]
        public async Task CreateTextFileAsync_3kB_FileOf3KBSize()
        {
            //Arrange
            await CreateTextFileAsyncTestBodyAsync(fileSizeInBytes: 3 * 1024);
        }

        [TestMethod]
        public async Task CreateTextFileAsync_1B_FileOf1BSize()
        {
            //Arrange
            await CreateTextFileAsyncTestBodyAsync(fileSizeInBytes: 1);
        }

        private async Task CreateTextFileAsyncTestBodyAsync(int fileSizeInBytes)
        {
            var tempFileName = GenerateTempFilePath();
            var sut = new TorrentFileHelper();

            //Act
            await sut.CreateTextFileAsync(tempFileName, fileSizeInBytes);

            //Assert
            var fi = new FileInfo(tempFileName);
            fi.Length.Should().Be(fileSizeInBytes);
        }
        #endregion

        #region CreateTorrentAsync
        [TestMethod]
        public async Task CreateTorrentAsync_TorrentWith1File_TorrentWith1File()
        {
            var sut = new TorrentFileHelper();
            var fileLoc1 = GenerateTempFilePath();
            await CreateFile(sut, fileLoc1, 32);

            var innerFiles = new CreateTorrentFileMapping[]
            {
                new CreateTorrentFileMapping { FileLocOnDisk = fileLoc1, FileLocInTorrent = "FileA.txt"},
            };

            var torrentName = "TestTorrent1File32kb";
            var torrentFileLoc = GenerateTempFilePath(extension: "torent");
            var newTorrentFile = CreateNewTorrentFile(innerFiles, torrentName);

            await CreateTorrentAsyncTestBodyAsync(sut, torrentFileLoc, newTorrentFile);
        }

        [TestMethod]
        public async Task CreateTorrentAsync_TorrentWith2Files_TorrentWith2Files()
        {
            var sut = new TorrentFileHelper();
            var fileLoc1 = GenerateTempFilePath();
            await CreateFile(sut, fileLoc1, 32);

            var fileLoc2 = GenerateTempFilePath();
            await CreateFile(sut, fileLoc2, 64);

            var innerFiles = new CreateTorrentFileMapping[]
            {
                new CreateTorrentFileMapping { FileLocOnDisk = fileLoc1, FileLocInTorrent = "FileA.txt"},
                new CreateTorrentFileMapping { FileLocOnDisk = fileLoc2, FileLocInTorrent = "FileB.txt"}
            };

            var torrentName = "TestTorrent2File32kbAnd64kb";
            var torrentFileLoc = GenerateTempFilePath(extension: "torent");
            var newTorrentFile = CreateNewTorrentFile(innerFiles, torrentName);

            await CreateTorrentAsyncTestBodyAsync(sut, torrentFileLoc, newTorrentFile);
        }

        private static async Task CreateFile(TorrentFileHelper sut, string fileLoc, int sizeInBytes)
        {
            var fileSize = sizeInBytes * 1024;
            await sut.CreateTextFileAsync(fileLoc, fileSize);
        }

        private static NewTorrentFile CreateNewTorrentFile(CreateTorrentFileMapping[] innerFiles, string torrentName)
        {
            return new NewTorrentFile()
            {
                Name = torrentName,
                IsPrivate = true,
                TrackerAnnounceUrl = "http://mytracker.org:2710/announce",
                FileMappings = innerFiles
            };
        }

        private static async Task CreateTorrentAsyncTestBodyAsync(TorrentFileHelper sut, string torrentFileLoc, NewTorrentFile newTorrentFile)
        {
            //Act 
            await sut.CreateTorrentAsync(torrentFileLoc, newTorrentFile);

            //Assert
            File.Exists(torrentFileLoc).Should().BeTrue();
            var torrentFile = await sut.ReadTorrentAsync(torrentFileLoc);
            torrentFile.Name.Should().Be(newTorrentFile.Name);
            torrentFile.IsPrivate.Should().Be(newTorrentFile.IsPrivate);
            torrentFile.TrackerAnnounceUrl.Should().Be(newTorrentFile.TrackerAnnounceUrl);
            torrentFile.InnerTorrentFiles.Count().Should().Be(newTorrentFile.FileMappings.Count());

            foreach (var fileMapping in newTorrentFile.FileMappings)
            {
                var innerTorrentFile = torrentFile.InnerTorrentFiles.Single(f => f.FileLocInTorrent == fileMapping.FileLocInTorrent);
                var actualSizeInbytes = new FileInfo(fileMapping.FileLocOnDisk).Length;
                innerTorrentFile.FileSizeInBytes.Should().Be(actualSizeInbytes);
            }
        }
        #endregion

        #region ReadTorrentAsync
        [TestMethod]
        public async Task ReadTorrentAsync_1File_TorrentWith1File()
        {
            var sut = new TorrentFileHelper();
            var torrentFileLocation = Path.Combine(TestFilesDir, "TorrentWith1File128kb.torent");
            var expectedTorrent = new ExistingTorrentFile
            {
                IsPrivate = true,
                Name = "TestTorrent1File128kb",
                TrackerAnnounceUrl = "http://mytracker.org:2710/announce",
                InnerTorrentFiles = new InnerTorrentFileInfo[]
                {
                    new InnerTorrentFileInfo { FileLocInTorrent = "FileA.txt", FileSizeInBytes = 128 * 1024 }
                }
            };

            await ReadTorrentAsyncTestBodyAsync(sut, torrentFileLocation, expectedTorrent);
        }

        [TestMethod]
        public async Task ReadTorrentAsync_2Files_TorrentWith2Files()
        {
            var sut = new TorrentFileHelper();
            var torrentFileLocation = Path.Combine(TestFilesDir, "TorrentWith2Files32kbAnd64kb.torent");
            var expectedTorrent = new ExistingTorrentFile
            {
                IsPrivate = true,
                Name = "TestTorrent2File32kbAnd64kb",
                TrackerAnnounceUrl = "http://mytracker.org:2710/announce",
                InnerTorrentFiles = new InnerTorrentFileInfo[]
                {
                    new InnerTorrentFileInfo { FileLocInTorrent = "FileA.txt", FileSizeInBytes = 32 * 1024 },
                    new InnerTorrentFileInfo { FileLocInTorrent = "FileB.txt", FileSizeInBytes = 64 * 1024 }
                }
            };

            await ReadTorrentAsyncTestBodyAsync(sut, torrentFileLocation, expectedTorrent);
        }

        private static async Task ReadTorrentAsyncTestBodyAsync(TorrentFileHelper sut, string torrentFileLocation, ExistingTorrentFile expectedTorrent)
        {
            //Act
            var existingTorrent = await sut.ReadTorrentAsync(torrentFileLocation);

            //Assert
            existingTorrent.Name.Should().Be(expectedTorrent.Name);
            existingTorrent.IsPrivate.Should().Be(expectedTorrent.IsPrivate);
            existingTorrent.TrackerAnnounceUrl.Should().Be(expectedTorrent.TrackerAnnounceUrl);
            existingTorrent.InnerTorrentFiles.Count().Should().Be(expectedTorrent.InnerTorrentFiles.Count());

            foreach (var expectedInnerTorrentFile in expectedTorrent.InnerTorrentFiles)
            {
                var existingInnerTorrentFile = existingTorrent.InnerTorrentFiles.Single(f => f.FileLocInTorrent == expectedInnerTorrentFile.FileLocInTorrent);
                existingInnerTorrentFile.FileSizeInBytes.Should().Be(expectedInnerTorrentFile.FileSizeInBytes);
            }
        }
        #endregion

        private string GenerateTempFilePath(string extension = "txt")
        {
            var tmpFileLoc = Path.Combine(Path.GetTempPath(), $"testfile_{Guid.NewGuid().ToString("N")}.{extension}");
            _createdTmpFiles.Add(tmpFileLoc);
            return tmpFileLoc;
        }
    }
}
