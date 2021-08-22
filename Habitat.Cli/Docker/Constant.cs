using Docker.DotNet.Models;
namespace Habitat.Cli.Docker
{
    public static class Constant
    {
        public static readonly Mount DockerBindingMount = new()
        {
            Source = "/var/run/docker.sock",
            Target = "/var/run/docker.sock",
            Type = "bind",
            ReadOnly = false
        };
    }
}
