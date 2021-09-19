using System.Collections.Generic;
using Docker.DotNet.Models;
using static Habitat.Cli.Utils.Objects;
using static Habitat.Cli.Utils.Strings;

namespace Habitat.Cli.Docker
{
    public static class DockerExtensions
    {
        public static HostConfig AttachMount(this HostConfig config, Mount? mount) {
            if (IsNull(mount)) return config;
            var configMounts = config.Mounts;
            if (IsNull(configMounts)) configMounts = new List<Mount>();

            configMounts.Add(mount);
            config.Mounts = configMounts;
            return config;
        }

        public static HostConfig AttachVolume(this HostConfig config, string target, VolumeResponse volume) {
            var mount = new Mount
            {
                Target = $"{target.TrimEnd('/')}/{volume.Name}",
                Source = volume.Name,
                Type = "volume"
            };
            return config.AttachMount(mount);
        }

        public static CreateContainerParameters AddEnv(this CreateContainerParameters containerParameters,
                                                       string name,
                                                       string? value) {
            if (IsBlank(value)) return containerParameters;
            var envs = containerParameters.Env;
            if (IsNull(envs)) {
                envs = new List<string>();
                containerParameters.Env = envs;
            }

            envs.Add($"{name}={value}");
            return containerParameters;
        }
    }
}
