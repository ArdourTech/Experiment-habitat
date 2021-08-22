using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Habitat.Cli
{
    public interface IDocker
    {
        Task<bool> ImageExistsAsync(string imageName);

        Task<bool> NetworkExistsAsync(string networkName);

        Task<string?> RunningContainerIdAsync(string containerName);

        Task<bool> IsContainerRunningAsync(string containerName);

        Task<string?> FindContainerIdAsync(string containerName);

        Task<string?> CreateContainerAsync(string image, string name);

        Task<bool> RunContainerAsync(string containerId);
        Task<bool> StopContainerAsync(string containerId);

        Task BindNetworksAsync(string containerId);

        Task BuildContainerAsync(FileInfo dockerfile,
                                 DirectoryInfo workingDirectory,
                                 string tag,
                                 Dictionary<string, string> buildArgs,
                                 bool noCache = false);

        Task<string?> GetEntryPointAsync(string containerName);
        Task<IDictionary<string, string>?> GetLabelsAsync(string containerName);
    }
}
