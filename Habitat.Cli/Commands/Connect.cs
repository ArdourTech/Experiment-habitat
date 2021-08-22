using System;
using System.Threading.Tasks;
using CommandDotNet;
using FluentValidation;
using FluentValidation.Attributes;
using static CommandDotNet.ExitCodes;

namespace Habitat.Cli.Commands
{
    [Validator(typeof(ConnectArgsValidator))]
    public class ConnectArgs : IArgumentModel
    {
        [Option(ShortName = "n",
                LongName = "name",
                Description = "Name for the Container")]
        public string Name { get; set; }
    }

    public class ConnectArgsValidator : AbstractValidator<ConnectArgs>
    {
        public ConnectArgsValidator() {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("The Name must not be blank or empty");
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    [Command(Usage = "%AppName% connect", Description = "Connects to a running Habitat Environment by name")]
    public class Connect
    {
        [DefaultMethod]
        public async Task<int> RunAsync(IDocker docker, ConnectArgs args) {
            var containerName = args.Name;
            if (!await docker.IsContainerRunningAsync(containerName)) {
                Log.Error($"Docker Container named {containerName} is not running.");
                return Error.Result;
            }
            var entryPoint = await docker.GetEntryPointAsync(containerName);
            Log.Info($"docker exec -it {containerName} {entryPoint}");
            return Success.Result;
        }
    }
}
