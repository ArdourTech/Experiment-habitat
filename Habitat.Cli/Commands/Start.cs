using System.Threading.Tasks;
using CommandDotNet;
using FluentValidation;
using FluentValidation.Attributes;
using static System.Environment;
using static CommandDotNet.ExitCodes;
using static Habitat.Cli.Utils.Strings;

namespace Habitat.Cli.Commands
{
    [Validator(typeof(StartArgsValidator))]
    public class StartArgs : IArgumentModel
    {
        [Option(ShortName = "i",
                LongName = "image",
                Description = "Image to Run")]
        public string Image { get; set; } = null!;

        [Option(ShortName = "n",
                LongName = "name",
                Description = "Name for the Container")]
        public string Name { get; set; } = "habitat";
    }

    public class StartArgsValidator : AbstractValidator<StartArgs>
    {
        public StartArgsValidator() {
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
        public async Task<int> RunAsync(IDocker docker, StartArgs args) {
            var containerName = args.Name;
            //Create Volumes and Networks if they do not exist

            if (await docker.IsContainerRunningAsync(containerName)) {
                Log.Info($"Docker Container named {containerName} is already running.");
                return Success.Result;
            }

            var containerImage = args.Image;
            if (!await docker.ImageExistsAsync(containerImage)) {
                Log.Error($"Docker Image {containerImage} was not found.");
                return Error.Result;
            }

            var containerId = await docker.FindContainerIdAsync(containerName);
            if (IsBlank(containerId)) {
                containerId = await docker.CreateContainerAsync(containerImage, containerName);
            }

            //TODO Mount all volumes to volume root
            var runContainer = await docker.RunContainerAsync(containerId!);
            //Bind other networks (if multiple)
            if (runContainer) {
                await docker.BindNetworksAsync(containerName);
            }

            return runContainer ? Success.Result : Error.Result;
        }
    }
}
