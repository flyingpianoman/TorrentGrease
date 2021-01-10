using Docker.DotNet;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestUtils;

namespace IntegrationTest.Symlink
{
    [TestClass]
    public class SymlinkTest
    {
        private static string GenerateTmpPath() => $"/tmp/{nameof(SymlinkTest)}_{Guid.NewGuid():N}/";

        [TestMethod]
        public async Task TestHardLinkCreationAndAssertion()
        {
            var dockerClient = DIContainer.Default.Get<DockerClient>();
            var id = await dockerClient.Containers.GetContainerIdByNameAsync(TestSettings.TorrentGreaseContainerName);

            var dir = GenerateTmpPath();
            var testFile = $"{dir}/test-source-file.txt";
            var testLinkFile = $"{dir}/test-link-file.txt";
            var fileContent = "test-content";

            await CreateFileAndLinkAsync(dockerClient, id, dir, testFile, testLinkFile, fileContent);

            //Assert
            var linkFileContent = await dockerClient.Containers.GetFileContentFromContainerAsync(id, testLinkFile);
            linkFileContent.Should().Be(fileContent);

            var fileInfo1 = await dockerClient.GetLinuxFileInfoInContainerAsync(id, testFile);
            var fileInfo2 = await dockerClient.GetLinuxFileInfoInContainerAsync(id, testLinkFile);
            fileInfo1.HardLinkCount.Should().Be(2);

            fileInfo1.InodeNumber.Should().NotBe(0);
            fileInfo1.FileSystemId.Should().NotBe(0);

            fileInfo1.Should().Be(fileInfo2);
        }

        private static async Task CreateFileAndLinkAsync(DockerClient dockerClient, string id, string dir, string testFile, string testLinkFile, string fileContent)
        {
            await dockerClient.CreateDirectoryStructureInContainerAsync(id, dir);
            await dockerClient.CreateFileInContainerAsync(id, testFile, fileContent);
            await dockerClient.CreateHardLinkInContainerAsync(id, testFile, testLinkFile);
        }

        private void A()
        {
            var process = new Process();
            //hosted by the application itself to not open a black cmd window
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
        }
    }
}
