using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommandDotNet;
using FluentValidation;
using FluentValidation.Attributes;
using static System.Char;
using static System.Environment;
using static CommandDotNet.BooleanMode;
using static CommandDotNet.ExitCodes;
using static Habitat.Cli.Utils.File;

namespace Habitat.Cli.Commands
{
    [Validator(typeof(BuildArgsValidator))]
    public class BuildArgs : IArgumentModel
    {
        [EnvVar("HABITAT_USER")]
        [Option(ShortName = "u",
                LongName = "user",
                Description = "Habitat User")]
        public string User { get; set; } = UserName.Replace(" ", "").ToLower();

        [EnvVar("HABITAT_USER_PASSWORD")]
        [Option(ShortName = "p",
                LongName = "password",
                Description = "Habitat User Password")]
        public Password Password { get; set; }

        [Option(ShortName = "d",
                LongName = "directory",
                Description = "Working Directory")]
        public DirectoryInfo WorkingDirectory { get; set; } = new(CurrentDirectory);

        [Option(ShortName = "f",
                LongName = "file",
                Description = "DockerFile")]
        public FileInfo Dockerfile { get; set; } = new(Path.Combine(CurrentDirectory, "Dockerfile"));

        [Option(BooleanMode = Implicit,
                LongName = "no-cache",
                Description = "No Cache")]
        public bool NoCache { get; set; } = false;

        [Option(ShortName = "t",
                LongName = "tag",
                Description = "Image Tag")]
        public string Tag { get; set; }
    }

    public class BuildArgsValidator : AbstractValidator<BuildArgs>
    {
        public BuildArgsValidator() {
            RuleFor(x => x.User)
                .NotEmpty()
                .Must(user => !user.All(IsWhiteSpace))
                .WithMessage("The Username must not be blank or contain any spaces");
            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("The Password must not be blank or empty");
            RuleFor(x => x.Tag)
                .NotEmpty()
                .WithMessage("The Tag must not be blank or empty");
            RuleFor(x => x.Dockerfile)
                .Must(Exists)
                .WithMessage("The Working Directory must contain the Dockerfile, or Path to Dockerfile must exist.");
            RuleFor(x => x.WorkingDirectory)
                .NotEmpty()
                .Must(Exists)
                .Must((args, info) => IsDescendant(args.Dockerfile, info))
                .WithMessage("The Dockerfile must exist within the working directory");
        }
    }

    [Command(Usage = "%AppName% build", Description = "Builds the Habitat Environment")]
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Build
    {
        // ReSharper disable once UnusedMember.Global
        [DefaultMethod]
        public async Task<int> RunAsync(IDocker docker, BuildArgs args) {
            var buildArgs = new Dictionary<string, string>
            {
                { "HABITAT_USER", args.User },
                { "HABITAT_USER_PASSWORD", args.Password.GetPassword() }
            };

            Log.Info($"Building Docker Image from {args.Dockerfile.FullName} " +
                     $"for user {args.User} " +
                     $"with context {args.WorkingDirectory.FullName}");
            await docker.BuildContainerAsync(args.Dockerfile,
                                             args.WorkingDirectory,
                                             args.Tag,
                                             buildArgs,
                                             args.NoCache);
            return Success.Result;
        }
    }
}
