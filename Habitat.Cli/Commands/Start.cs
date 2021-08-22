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

        [Option(LongName = "network",
                Description = "Network to connect the Container to. This value is only used during Container creation")]
        public string? Network { get; set; } = "";

        [Option(LongName = "with-x11-display",
                Description =
                    "Adds an Environment variable to bind the X11 Display to the host during Container creation")]
        public bool WithX11Display { get; set; } = false;

        [Option(LongName = "with-docker",
                Description =
                    "Binds the Host Docker Socket to the running Container; allowing access to the Host's Docker Engine")]
        public bool WithDocker { get; set; } = false;
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
            //TODO Find Volumes and Networks Based on Container Labels
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

            var networkName = args.Network;
            if (IsNotBlank(networkName) && !await docker.NetworkExistsAsync(networkName!)) {
                Log.Error($"Docker Network {networkName} was not found." +
                          $"{NewLine}" +
                          $"You can create one by running `docker network create --driver bridge {networkName}`");
                return Error.Result;
            }

            var containerId = await docker.FindContainerIdAsync(containerName);
            if (IsBlank(containerId))
                containerId = await docker.CreateContainerAsync(containerImage,
                                                                containerName,
                                                                args.WithX11Display,
                                                                args.WithDocker,
                                                                networkName);

            //Mount all volumes to volume root
            var runContainer = await docker.RunContainerAsync(containerId!);
            //Bind other networks (if multiple)

            return runContainer ? Success.Result : Error.Result;
        }
    }
}
