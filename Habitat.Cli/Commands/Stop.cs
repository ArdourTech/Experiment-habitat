using System.Threading.Tasks;
using CommandDotNet;
using FluentValidation;
using FluentValidation.Attributes;
using static CommandDotNet.ExitCodes;
using static Habitat.Cli.Utils.Strings;

namespace Habitat.Cli.Commands
{
    [Validator(typeof(StopArgsValidator))]
    public class StopArgs : IArgumentModel
    {
        [Option(ShortName = "n",
                LongName = "name",
                Description = "Name of the Container to Stop")]
        public string Name { get; set; } = "habitat";
    }

    public class StopArgsValidator : AbstractValidator<StopArgs>
    {
        public StopArgsValidator() {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("The Name must not be blank or empty");
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    [Command(Usage = "%AppName% stop", Description = "Stops a named Habitat Environment")]
    public class Stop
    {
        [DefaultMethod]
        public async Task<int> RunAsync(IDocker docker, StopArgs args) {
            var containerName = args.Name;
            var runningContainerId = await docker.RunningContainerIdAsync(containerName);
            if (IsBlank(runningContainerId)) {
                Log.Debug($"No running Docker Container name {containerName}");
                return Success.Result;
            }

            Log.Debug($"Stopping Docker Container {runningContainerId}");
            await docker.StopContainerAsync(runningContainerId!);
            return Success.Result;
        }
    }
}
