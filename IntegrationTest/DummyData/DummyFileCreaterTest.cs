using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestUtils.Torrent;

namespace IntegrationTest.DummyData
{
    [TestClass]
    public class DummyFileCreaterTest
    {
        private IList<string> _filesToRemove;

        [TestInitialize]
        public void TestInit()
        {
            _filesToRemove = new List<string>();
        }

        [TestCleanup]
        public void Cleanup()
        {
            foreach (var file in _filesToRemove)
            {
                File.Delete(file);
            }
        }

        [TestMethod]
        public async Task MyTestMethodAsync()
        {
            var nrOfBytesToWrite = 10485760;
            var sut = new TorrentFileHelper();

            var tmpFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            _filesToRemove.Add(tmpFile);

            //Act
            var sw = new Stopwatch();
            sw.Start();
            await sut.CreateTextFileAsync(tmpFile, nrOfBytesToWrite, '*');
            sw.Stop();

            //Assert
            sw.ElapsedMilliseconds.Should().BeLessThan(100);

            new FileInfo(tmpFile).Length.Should().Be(nrOfBytesToWrite);
            await AssertFirstCharAsync(tmpFile, '*');
        }

        private static async Task AssertFirstCharAsync(string tmpFile, char firstChar)
        {
            var readBuffer = new byte[1];
            using var fs = new FileStream(tmpFile, FileMode.Open);
            await fs.ReadAsync(readBuffer.AsMemory(0, 1));
            var actualChars = UTF8Encoding.UTF8.GetChars(readBuffer);
            actualChars[0].Should().Be(firstChar);
        }
    }
}
