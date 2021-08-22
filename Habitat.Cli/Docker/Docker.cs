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
using static Habitat.Cli.Docker.Query;
using static Habitat.Cli.Docker.Constant;

namespace Habitat.Cli.Docker
{
    public class Docker : IDocker
    {
        private readonly CancellationToken _cancellationToken;
        private readonly DockerClient _instance;

        public Docker(CancellationToken cancellationToken = default) {
            _cancellationToken = cancellationToken;
            _instance = new DockerClientConfiguration().CreateClient();
        }

        public async Task<string?> CreateContainerAsync(string image, string name) {
            var imageDefinition = await FindImageAsync(image);
            var labels = imageDefinition!.Labels;
            Log.Info($"Creating Container for {image} named {name}");
            var hostConfig = new HostConfig();
            if (labels.ContainsKey("HABITAT_WITH_DOCKER")) {
                Log.Debug($"Binding Host Docker Sock to Container {name}");
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
                Log.Debug($"Binding Host X11 Display to Container {name}");
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
            var container = await FindContainerAsync(containerName, "running");
            return container?.ID;
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

        private async Task<ContainerListResponse?> FindContainerAsync(string containerName,
                                                                      string status = null!) {
            var filters = new Dictionary<string, IDictionary<string, bool>>();
            AddBasicFilter(filters, "name", containerName);
            if (IsNotBlank(status)) {
                AddBasicFilter(filters, "status", status);
            }

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

        public async Task BindNetworksAsync(string containerName) {
            var container = await FindContainerAsync(containerName, "running");
            if (IsNull(container)) return;
            container!.Labels.TryGetValue("HABITAT_NETWORKS", out var networks);
            if (IsBlank(networks)) {
                Log.Debug($"No HABITAT_NETWORKS found to attach {containerName} to");
                return;
            }

            var networkNames = networks!
                               .Split(',')
                               .Select(s => s.Trim());
            foreach (var networkName in networkNames) {
                var network = await FindNetworkAsync(networkName);
                if (IsNull(network)) {
                    network = await CreateBridgeNetworkAsync(networkName);
                }

                await AttachNetworkAsync(container, network!);
            }
        }

        private async Task<NetworkResponse?> CreateBridgeNetworkAsync(string networkName) {
            var createParams = new NetworksCreateParameters
            {
                Name = networkName,
                Driver = "bridge"
            };
            var response = await _instance.Networks.CreateNetworkAsync(createParams, _cancellationToken);
            Log.Debug($"Created Network {networkName}: {response.ID}");
            return await FindNetworkAsync(networkName);
        }

        private async Task<NetworkResponse?> FindNetworkAsync(string networkName) {
            var filters = new Dictionary<string, IDictionary<string, bool>>();
            AddBasicFilter(filters, "name", networkName);
            var listParams = new NetworksListParameters
            {
                Filters = filters
            };
            var networks = await _instance.Networks.ListNetworksAsync(listParams, _cancellationToken);
            return networks.FirstOrDefault(NetworkNamed(networkName));
        }

        private async Task AttachNetworkAsync(ContainerListResponse container, NetworkResponse network) {
            var aliases = container.Names.Select(n => n.TrimStart('/')).ToList();
            Log.Debug($"Attaching {aliases.First()} to Network {network.Name}");
            var connectParams = new NetworkConnectParameters
            {
                Container = container.ID,
                EndpointConfig = new EndpointSettings
                {
                    Aliases = aliases
                }
            };
            await _instance.Networks.ConnectNetworkAsync(network.ID, connectParams, _cancellationToken);
        }
    }
}
