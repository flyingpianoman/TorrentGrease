using Docker.DotNet;
using Docker.DotNet.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SpecificationTest.Crosscutting
{
    public static class DockerClientExtensions
    {
        internal static void RegisterDockerClient(this DIContainer diContainer)
        {
            var dockerAddress = RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                                ? "unix:///var/run/docker.sock"
                                : "npipe://./pipe/docker_engine"; //windows addr
            using var dockerClientConfiguration = new DockerClientConfiguration(new Uri(dockerAddress));
            var dockerClient = dockerClientConfiguration.CreateClient();
            diContainer.Register(dockerClient);
        }

        internal static async Task<string> GetContainerIdByNameAsync(this IContainerOperations containerOperations,
            string containerName)
        {
            var containers = await containerOperations.ListContainersAsync(new ContainersListParameters()).ConfigureAwait(false);
            return containers.Single(c => c.State == "running" && c.Names.Contains("/" + containerName)).ID;
        }

        internal static async Task<GetArchiveFromContainerResponse> GetArchiveFromContainerAsync(this IContainerOperations containerOperations,
            string sourcePath, string containerId)
        {
            return await containerOperations.GetArchiveFromContainerAsync(containerId, new GetArchiveFromContainerParameters
            {
                Path = sourcePath
            }, false).ConfigureAwait(false);
        }

        internal static async Task<GetArchiveFromContainerResponse> GetArchiveFromContainerByNameAsync(this IContainerOperations containerOperations,
            string sourcePath, string containerName)
        {
            string containerId = await containerOperations.GetContainerIdByNameAsync(containerName).ConfigureAwait(false);
            return await containerOperations.GetArchiveFromContainerAsync(sourcePath, containerId).ConfigureAwait(false);
        }

        internal static async Task UploadTarredFileToContainerAsync(this IContainerOperations containerOperations,
            MemoryStream tarredFileStream, string containerName, string destinationPath)
        {
            var id = await containerOperations.GetContainerIdByNameAsync(containerName).ConfigureAwait(false);
            tarredFileStream.Position = 0;
            await containerOperations.ExtractArchiveToContainerAsync(
                id,
                new ContainerPathStatParameters { AllowOverwriteDirWithFile = true, Path = destinationPath },
                tarredFileStream).ConfigureAwait(false);
        }
    }
}
