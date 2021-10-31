using Docker.DotNet;
using Docker.DotNet.Models;
using Polly;
using SpecificationTest.Crosscutting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestUtils
{
    public static class DockerClientExtensions
    {
        public static void RegisterDockerClient(this DIContainer diContainer)
        {
            var dockerAddress = RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                                ? "unix:///var/run/docker.sock"
                                : "npipe://./pipe/docker_engine"; //windows addr
            using var dockerClientConfiguration = new DockerClientConfiguration(new Uri(dockerAddress));
            var dockerClient = dockerClientConfiguration.CreateClient();
            diContainer.Register(dockerClient);
        }

        public static async Task<string> GetContainerIdByNameAsync(this IContainerOperations containerOperations,
            string containerName)
        {
            var containers = await containerOperations.ListContainersAsync(new ContainersListParameters()).ConfigureAwait(false);
            return containers.Single(c => c.State == "running" && c.Names.Contains("/" + containerName)).ID;
        }

        public static async Task<string> GetFileContentFromContainerAsync(this IContainerOperations containerOperations,
            string containerId, string sourcePath)
        {
            var response = await containerOperations.GetArchiveFromContainerAsync(containerId, new GetArchiveFromContainerParameters
            {
                Path = sourcePath
            }, false).ConfigureAwait(false);

            return await ArchiveHelper.ExtractSingleFileFromTarToStringAsync(response.Stream);
        }

        public static async Task<GetArchiveFromContainerResponse> GetArchiveFromContainerAsync(this IContainerOperations containerOperations,
            string sourcePath, string containerId)
        {
            return await containerOperations.GetArchiveFromContainerAsync(containerId, new GetArchiveFromContainerParameters
            {
                Path = sourcePath
            }, false).ConfigureAwait(false);
        }

        public static async Task<GetArchiveFromContainerResponse> GetArchiveFromContainerByNameAsync(this IContainerOperations containerOperations,
            string sourcePath, string containerName)
        {
            string containerId = await containerOperations.GetContainerIdByNameAsync(containerName).ConfigureAwait(false);
            return await containerOperations.GetArchiveFromContainerAsync(sourcePath, containerId).ConfigureAwait(false);
        }

        public static async Task UploadTarredFileToContainerAsync(this IContainerOperations containerOperations,
            MemoryStream tarredFileStream, string containerName, string destinationPath)
        {
            var id = await containerOperations.GetContainerIdByNameAsync(containerName).ConfigureAwait(false);
            tarredFileStream.Position = 0;
            await containerOperations.ExtractArchiveToContainerAsync(
                id,
                new ContainerPathStatParameters { AllowOverwriteDirWithFile = true, Path = destinationPath },
                tarredFileStream).ConfigureAwait(false);
        }

        public static async Task CreateDirectoryStructureInContainerAsync(this DockerClient dockerClient,
            string containerId, string fullPath)
        {
            var targetDirParts = fullPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var mkDirCommands = new List<string>();
            var dir = "";
            var structureExists = true;

            foreach (var targetDirPart in targetDirParts)
            {
                var parentDir = String.IsNullOrEmpty(dir)? "/" : dir;
                dir += "/" + targetDirPart;

                if (structureExists)
                {
                    structureExists = await ExecuteSHCommandAsync(dockerClient, containerId, $"ls '{parentDir}' | grep -wc '{targetDirPart}'") == "1";
                }

                if (!structureExists)
                {
                    mkDirCommands.Add($"mkdir {dir}");
                }
            }

            var commandToExecute = string.Join(" || ", mkDirCommands);
            await ExecuteSHCommandAsync(dockerClient, containerId, commandToExecute).ConfigureAwait(false);
        }

        public static async Task MoveFileInContainerAsync(this DockerClient dockerClient,
            string containerId, string from, string to)
        {
            await ExecuteSHCommandAsync(dockerClient, containerId, $"mv \"{from}\" \"{to}\"").ConfigureAwait(false);
        }

        public static async Task EmptyDirInContainerAsync(this DockerClient dockerClient,
            string containerId, string dirPath)
        {
            await ExecuteSHCommandAsync(dockerClient, containerId, $"find \"{dirPath}\" -mindepth 1 -delete").ConfigureAwait(false);
        }

        public static async Task CreateFileInContainerAsync(this DockerClient dockerClient,
            string containerId, string fullPath, string fileContent)
        {
            var escapedFileContent = fileContent.Replace("\"", "\\\"");
            var commandToExecute = $"printf {escapedFileContent} > {fullPath}";

            await ExecuteSHCommandAsync(dockerClient, containerId, commandToExecute).ConfigureAwait(false);
        }

        public static async Task<LinuxFileInfo> GetLinuxFileInfoInContainerAsync(this DockerClient dockerClient,
            string containerId, string fullPath)
        {
            var escapedFullPath = fullPath.Replace("\"", "\\\"");
            var commandToExecute = $"stat -c %d-%i-%h \"{escapedFullPath}\"";

            var response = await ExecuteSHCommandWithResponseAsync(dockerClient, containerId, commandToExecute).ConfigureAwait(false);
            var parts = response.Split('-');
            return new LinuxFileInfo
            {
                FileSystemId = Int32.Parse(parts[0]),
                InodeNumber = Int32.Parse(parts[1]),
                HardLinkCount = Int32.Parse(parts[2]),
            };
        }

        public static async Task CreateHardLinkInContainerAsync(this DockerClient dockerClient,
            string containerId, string sourceFile, string link)
        {
            var escapedSourceFile = sourceFile.Replace("\"", "\\\"");
            var escapedlink = link.Replace("\"", "\\\"");
            var commandToExecute = $"ln \"{escapedSourceFile}\" \"{escapedlink}\"";

            await ExecuteSHCommandAsync(dockerClient, containerId, commandToExecute).ConfigureAwait(false);
        }

        private static async Task<string> ExecuteSHCommandAsync(DockerClient dockerClient, string containerId, string commandToExecute)
        {
            var execCommandResponse = await CreateSHExecCommandAsync(dockerClient, containerId, commandToExecute).ConfigureAwait(false);
            using var multiplexStream = await dockerClient.Exec.StartAndAttachContainerExecAsync(execCommandResponse.ID, tty: false).ConfigureAwait(false);
            (string output, string stderr) = await Policy
                .TimeoutAsync(seconds: 30)
                .ExecuteAsync<(string output, string stderr)>(async (CancellationToken ct) => await multiplexStream.ReadOutputToEndAsync(ct).ConfigureAwait(false), new CancellationToken()).ConfigureAwait(false);

            if(!string.IsNullOrEmpty(stderr))
            {
                throw new InvalidOperationException("The command returned the following error: " + stderr);
            }

            return output.EndsWith("\n")? output[0..^1] : output;
        }

        private static async Task<string> ExecuteSHCommandWithResponseAsync(DockerClient dockerClient, string containerId, string commandToExecute)
        {
            var execCommandResponse = await CreateSHExecCommandAsync(dockerClient, containerId, commandToExecute).ConfigureAwait(false);
            using var stream = await dockerClient.Exec.StartAndAttachContainerExecAsync(execCommandResponse.ID, tty: true).ConfigureAwait(false);
            var (output, errors) = await stream.ReadOutputToEndAsync(default);
            return output + errors;
        }

        private static async Task<ContainerExecCreateResponse> CreateSHExecCommandAsync(DockerClient dockerClient, string containerId, string commandToExecute)
        {
            return await dockerClient.Exec.ExecCreateContainerAsync(containerId, new ContainerExecCreateParameters
            {
                Cmd = new List<string>
                        {
                            "sh",
                            "-c",
                            commandToExecute
                        },
                AttachStderr = true,
                AttachStdout = true
            }).ConfigureAwait(false);
        }
    }
}
