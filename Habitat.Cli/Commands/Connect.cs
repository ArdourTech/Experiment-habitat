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
        [Option(
            ShortName = "n",
            LongName = "name",
            Description = "Name for the Container")]
        public string Name { get; set; } = "habitat";

        [Option(
            ShortName = "c",
            LongName = "command",
            Description = "Command to exec")]
        public string Command { get; set; } = "fish";
    }

    public class ConnectArgsValidator : AbstractValidator<ConnectArgs>
    {
        public ConnectArgsValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("The Name must not be blank or empty");

            RuleFor(x => x.Command)
                .NotEmpty()
                .WithMessage("The Command must not be blank or empty");
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    [Command(Usage = "%AppName% connect", Description = "Connects to a running Habitat Environment by name")]
    public class Connect
    {
        [DefaultMethod]
        public async Task<int> RunAsync(CommandContext context, ConnectArgs args)
        {
            var docker = context.Services.GetOrThrow<IDocker>();
            if (!await docker.IsContainerRunningAsync(args.Name))
            {
                Log.Error($"Docker Container named {args.Name} is not running.");
                return Error.Result;
            }

            Log.Info($"docker exec -it {args.Name} fish");
            return Success.Result;
        }
    }
}
