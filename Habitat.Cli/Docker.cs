using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Habitat.Cli.Utils;
using Newtonsoft.Json.Linq;

namespace Habitat.Cli
{
    public class Docker : IDocker
    {
        private readonly CancellationToken _cancellationToken;
        private readonly DockerClient _instance;

        public Docker(CancellationToken cancellationToken = default)
        {
            _cancellationToken = cancellationToken;
            _instance = new DockerClientConfiguration().CreateClient();
        }

        public async Task<string> CreateContainerAsync(string image, string name)
        {
            Log.Info($"Creating Container for {image} named {name}");
            var createParams = new CreateContainerParameters
            {
                Name = name,
                Env = new List<string> { "DISPLAY=docker.host.internal:0" },
                Image = image,
                Tty = true,
                AttachStderr = true,
                AttachStdin = true,
                AttachStdout = true
            };
            var container = await _instance.Containers.CreateContainerAsync(createParams, _cancellationToken);
            return container?.ID;
        }

        public async Task<bool> RunContainerAsync(string containerId)
        {
            Log.Info($"Starting Container {containerId}");
            var startParams = new ContainerStartParameters();
            return await _instance.Containers
                .StartContainerAsync(containerId, startParams, _cancellationToken);
        }

        public async Task<bool> StopContainerAsync(string containerId)
        {
            Log.Info($"Stopping Container {containerId}");
            var stopParams = new ContainerStopParameters
            {
                WaitBeforeKillSeconds = 30
            };
            return await _instance.Containers.StopContainerAsync(containerId, stopParams, _cancellationToken);
        }

        public async Task BuildContainerAsync(
            FileInfo dockerfile,
            DirectoryInfo workingDirectory,
            string tag,
            Dictionary<string, string> buildArgs,
            bool noCache = false)
        {
            var dockerfileName = dockerfile.FullName
                .Replace(workingDirectory.FullName, "")
                .Replace('\\', '/').TrimStart('/');
            var dockerBuildArgs = new ImageBuildParameters
            {
                Dockerfile = dockerfileName,
                BuildArgs = buildArgs,
                NoCache = noCache,
                Tags = new List<string> { tag },
            };
            await using var tarball = Zip.TarballDirectory(workingDirectory.FullName, _cancellationToken);
            var outputStream =
                await _instance.Images.BuildImageFromDockerfileAsync(tarball, dockerBuildArgs, _cancellationToken);
            using var reader = new StreamReader(outputStream);
            string line;
            while (!reader.EndOfStream && Objects.NonNull(line = await reader.ReadLineAsync()))
            {
                var stream = JObject.Parse(line!).SelectToken("stream");
                if (Objects.IsNull(stream)) continue;
                var value = stream!.Value<string>();
                Log.Info(value);
            }
        }

        public async Task<bool> ImageExistsAsync(string imageName)
        {
            Log.Debug($"Looking for Images matching name {imageName}");
            var listParams = new ImagesListParameters
            {
                MatchName = imageName,
                All = false
            };
            var images = await _instance.Images.ListImagesAsync(listParams, _cancellationToken);
            return images.Any(image => image.RepoTags.Contains(listParams.MatchName));
        }

        public async Task<string> RunningContainerIdAsync(string containerName)
        {
            Log.Debug($"Looking for existing running Container named {containerName}");
            var listParams = new ContainersListParameters
            {
                All = true,
                Filters = new Dictionary<string, IDictionary<string, bool>>
                {
                    {
                        "name", new Dictionary<string, bool>
                        {
                            { containerName, true }
                        }
                    },
                    {
                        "status", new Dictionary<string, bool>
                        {
                            { "running", true }
                        }
                    }
                }
            };
            var results = await _instance.Containers.ListContainersAsync(listParams, _cancellationToken);
            return results.FirstOrDefault(ContainerNamed(containerName))?.ID;
        }

        public async Task<string> FindContainerIdAsync(string containerName)
        {
            Log.Info($"Checking for existing Container named {containerName}");
            var listParams = new ContainersListParameters
            {
                All = true,
                Filters = new Dictionary<string, IDictionary<string, bool>>
                {
                    {
                        "name", new Dictionary<string, bool>
                        {
                            { containerName, true }
                        }
                    }
                }
            };
            var containers = await _instance.Containers.ListContainersAsync(listParams, _cancellationToken);
            return containers.FirstOrDefault(ContainerNamed(containerName))?.ID;
        }

        public async Task<bool> IsContainerRunningAsync(string containerName)
        {
            var runningContainerId = await RunningContainerIdAsync(containerName);
            return Strings.IsNotBlank(runningContainerId);
        }

        private static Func<ContainerListResponse, bool> ContainerNamed(string name)
        {
            return l => l.Names.Contains($"/{name}");
        }
    }
}
