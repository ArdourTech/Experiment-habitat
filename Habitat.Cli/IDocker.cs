using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Habitat.Cli
{
    public interface IDocker
    {
        Task<bool> ImageExistsAsync(string imageName);

        Task<string> RunningContainerIdAsync(string containerName);

        Task<bool> IsContainerRunningAsync(string containerName);

        Task<string> FindContainerIdAsync(string containerName);

        Task<string> CreateContainerAsync(
            string image,
            string name,
            bool withX11DisplayBinding = false,
            bool withDockerBinding = false);

        Task<bool> RunContainerAsync(string containerId);
        Task<bool> StopContainerAsync(string containerId);

        Task BuildContainerAsync(
            FileInfo dockerfile,
            DirectoryInfo workingDirectory,
            string tag,
            Dictionary<string, string> buildArgs,
            bool noCache = false);
    }
}
