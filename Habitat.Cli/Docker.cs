using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Newtonsoft.Json.Linq;
using static Habitat.Cli.Utils.Objects;
using static Habitat.Cli.Utils.Strings;
using static Habitat.Cli.Utils.Zip;

namespace Habitat.Cli
{
    public class Docker : IDocker
    {
        private static readonly Mount DockerBindingMount = new()
        {
            Source = "/var/run/docker.sock",
            Target = "/var/run/docker.sock",
            Type = "bind",
            ReadOnly = false
        };

        private readonly CancellationToken _cancellationToken;
        private readonly DockerClient _instance;

        public Docker(CancellationToken cancellationToken = default) {
            _cancellationToken = cancellationToken;
            _instance = new DockerClientConfiguration().CreateClient();
        }

        public async Task<string?> CreateContainerAsync(string image, string name, string? networkName = null) {
            var imageDefinition = await FindImageAsync(image);
            var labels = imageDefinition!.Labels;
            Log.Info($"Creating Container for {image} named {name}");
            //FIXME Network definitions from within the container
            var hostConfig = new HostConfig()
                .AttachNetwork(networkName);

            if (labels.ContainsKey("HABITAT_WITH_DOCKER")) {
                hostConfig.AttachMount(DockerBindingMount);
            }

            var createParams = new CreateContainerParameters
            {
                Name = name,
                Image = image,
                Tty = true,
                AttachStderr = true,
                AttachStdin = true,
                AttachStdout = true,
                HostConfig = hostConfig
            };
            if (labels.ContainsKey("HABITAT_WITH_X11")) {
                createParams.AddEnv("DISPLAY", "host.docker.internal:0");
            }

            var container = await _instance.Containers.CreateContainerAsync(createParams, _cancellationToken);
            return container?.ID;
        }

        public async Task<bool> RunContainerAsync(string containerId) {
            Log.Info($"Starting Container {containerId}");
            var startParams = new ContainerStartParameters();
            return await _instance
                         .Containers
                         .StartContainerAsync(containerId, startParams, _cancellationToken);
        }

        public async Task<bool> StopContainerAsync(string containerId) {
            Log.Info($"Stopping Container {containerId}");
            var stopParams = new ContainerStopParameters
            {
                WaitBeforeKillSeconds = 30
            };
            return await _instance.Containers.StopContainerAsync(containerId, stopParams, _cancellationToken);
        }

        public async Task BuildContainerAsync(FileInfo dockerfile,
                                              DirectoryInfo workingDirectory,
                                              string tag,
                                              Dictionary<string, string> buildArgs,
                                              bool noCache = false) {
            var dockerfileName = dockerfile.FullName
                                           .Replace(workingDirectory.FullName, "")
                                           .Replace('\\', '/')
                                           .TrimStart('/');
            var dockerBuildArgs = new ImageBuildParameters
            {
                Dockerfile = dockerfileName,
                BuildArgs = buildArgs,
                NoCache = noCache,
                Tags = new List<string> { tag }
            };
            await using var tarball = TarballDirectory(workingDirectory.FullName, _cancellationToken);
            var outputStream = await _instance
                                     .Images
                                     .BuildImageFromDockerfileAsync(tarball, dockerBuildArgs, _cancellationToken);
            using var reader = new StreamReader(outputStream);
            string? line;
            while (!reader.EndOfStream && NonNull(line = await reader.ReadLineAsync())) {
                var stream = JObject.Parse(line!).SelectToken("stream");
                if (IsNull(stream)) continue;
                var value = stream!.Value<string>();
                Log.Info(value);
            }
        }

        public async Task<bool> ImageExistsAsync(string imageName) {
            var image = await FindImageAsync(imageName);
            return NonNull(image);
        }

        public async Task<bool> NetworkExistsAsync(string networkName) {
            Log.Debug($"Looking for Networks named {networkName}");
            var filters = new Dictionary<string, IDictionary<string, bool>>();
            AddBasicFilter(filters, "name", networkName);
            var networkListParams = new NetworksListParameters
            {
                Filters = filters
            };
            var networks = await _instance.Networks.ListNetworksAsync(networkListParams, _cancellationToken);
            return networks.Any(network => network.Name.Equals(networkName));
        }

        public async Task<string?> RunningContainerIdAsync(string containerName) {
            Log.Debug($"Looking for existing running Container named {containerName}");
            var filters = new Dictionary<string, IDictionary<string, bool>>();
            AddBasicFilter(filters, "name", containerName);
            AddBasicFilter(filters, "status", "running");
            var listParams = new ContainersListParameters
            {
                All = true,
                Filters = filters
            };
            var results = await _instance.Containers.ListContainersAsync(listParams, _cancellationToken);
            return results.FirstOrDefault(ContainerNamed(containerName))?.ID;
        }

        public async Task<string?> FindContainerIdAsync(string containerName) {
            Log.Info($"Checking for existing Container named {containerName}");
            var container = await FindContainerAsync(containerName);
            return container?.ID;
        }

        public async Task<bool> IsContainerRunningAsync(string containerName) {
            var runningContainerId = await RunningContainerIdAsync(containerName);
            return IsNotBlank(runningContainerId);
        }

        public async Task<string?> GetEntryPointAsync(string containerName) {
            var container = await FindContainerAsync(containerName);
            return container?.Command;
        }

        public async Task<IDictionary<string, string>?> GetLabelsAsync(string containerName) {
            var container = await FindContainerAsync(containerName);
            return container?.Labels;
        }

        private async Task<ContainerListResponse?> FindContainerAsync(string containerName) {
            var filters = new Dictionary<string, IDictionary<string, bool>>();
            AddBasicFilter(filters, "name", containerName);
            var listParams = new ContainersListParameters
            {
                All = true,
                Filters = filters
            };
            var containers = await _instance.Containers.ListContainersAsync(listParams, _cancellationToken);
            return containers.FirstOrDefault(ContainerNamed(containerName));
        }

        private async Task<ImagesListResponse?> FindImageAsync(string imageName) {
            Log.Debug($"Looking for Images matching name {imageName}");
            var listParams = new ImagesListParameters
            {
                MatchName = imageName,
                All = false
            };
            var images = await _instance.Images.ListImagesAsync(listParams, _cancellationToken);
            return images.FirstOrDefault(ImageNamed(imageName));
        }

        private static void AddBasicFilter(IDictionary<string, IDictionary<string, bool>> filters,
                                           string key,
                                           string value) {
            if (IsBlank(value)) return;
            var nameFilter = new Dictionary<string, bool>
            {
                { value, true }
            };
            filters.Add(key, nameFilter);
        }

        private static Func<ContainerListResponse, bool> ContainerNamed(string name) {
            return l => l.Names.Contains($"/{name}");
        }

        private static Func<ImagesListResponse, bool> ImageNamed(string name) {
            return i => i.RepoTags.Contains($"{name}");
        }
    }
}
