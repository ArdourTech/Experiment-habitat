using System.Threading.Tasks;
using CommandDotNet;
using FluentValidation;
using FluentValidation.Attributes;
using static Habitat.Cli.Utils.Strings;

namespace Habitat.Cli.Commands
{
    [Validator(typeof(StartArgsValidator))]
    public class StartArgs : IArgumentModel
    {
        [Option(
            ShortName = "i",
            LongName = "image",
            Description = "Image to Run")]
        public string Image { get; set; }

        [Option(
            ShortName = "n",
            LongName = "name",
            Description = "Name for the Container")]
        public string Name { get; set; } = "habitat";
    }

    public class StartArgsValidator : AbstractValidator<StartArgs>
    {
        public StartArgsValidator()
        {
            RuleFor(x => x.Image)
                .NotEmpty()
                .WithMessage("The Image must not be blank or empty");

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("The Name must not be blank or empty");
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    [Command(Usage = "%AppName% start", Description = "Starts a Habitat Environment")]
    public class Start
    {
        [DefaultMethod]
        public async Task<int> RunAsync(CommandContext context, StartArgs args)
        {
            var docker = context.Services.GetOrThrow<IDocker>();
            if (await docker.IsContainerRunningAsync(args.Name))
            {
                Log.Info($"Docker Container named {args.Name} is already running.");
                return 0;
            }

            if (!await docker.ImageExistsAsync(args.Image))
            {
                Log.Error($"Docker Image {args.Image} was not found.");
                return 1;
            }

            var containerId = await docker.FindContainerIdAsync(args.Name);
            if (IsBlank(containerId)) containerId = await docker.CreateContainerAsync(args.Image, args.Name);

            var runContainer = await docker.RunContainerAsync(containerId);
            return runContainer ? 0 : 1;
        }
    }
}
