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
#pragma warning disable IDE0067 // Dispose objects before losing scope -> disposed by DI container
        public static void RegisterDockerClient(this DIContainer diContainer)
        {
            var dockerAddress = RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                                ? "unix:///var/run/docker.sock"
                                : "npipe://./pipe/docker_engine"; //windows addr
            var dockerClient = new DockerClientConfiguration(new Uri(dockerAddress)).CreateClient();
            diContainer.Register(dockerClient);
        }
#pragma warning restore IDE0067 // Dispose objects before losing scope

        public static async Task<string> GetContainerIdByNameAsync(this IContainerOperations containerOperations, 
            string containerName)
        {
            var containers = await containerOperations.ListContainersAsync(new ContainersListParameters());
            return containers.Single(c => c.State == "running" && c.Names.Contains("/" + containerName)).ID;
        }

        public static async Task<GetArchiveFromContainerResponse> GetArchiveFromContainerAsync(this IContainerOperations containerOperations, 
            string sourcePath, string containerId)
        {
            return await containerOperations.GetArchiveFromContainerAsync(containerId, new GetArchiveFromContainerParameters
            {
                Path = sourcePath
            }, false);
        }

        public static async Task<GetArchiveFromContainerResponse> GetArchiveFromContainerByNameAsync(this IContainerOperations containerOperations,
            string sourcePath, string containerName)
        {
            string containerId = await containerOperations.GetContainerIdByNameAsync(containerName);
            var archiveData = await containerOperations.GetArchiveFromContainerAsync(sourcePath, containerId);
            return archiveData;
        }

        public static async Task UploadTarredFileToContainerAsync(this IContainerOperations containerOperations, 
            MemoryStream tarredFileStream, string containerName, string destinationPath)
        {
            var id = await containerOperations.GetContainerIdByNameAsync(containerName);
            tarredFileStream.Position = 0;
            await containerOperations.ExtractArchiveToContainerAsync(
                id,
                new ContainerPathStatParameters { AllowOverwriteDirWithFile = true, Path = destinationPath },
                tarredFileStream);
        }
    }
}
