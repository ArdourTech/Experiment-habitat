using System;
using System.Threading.Tasks;
using CommandDotNet;
using CommandDotNet.Execution;
using CommandDotNet.Rendering;
using static CommandDotNet.Builders.BuildEvents;

namespace Habitat.Cli.Builders
{
    internal static class VersionMiddleware
    {
        private static readonly string VersionOptionName = Constants.VersionOptionName;
        private static string _appVersion = "";

        internal static Action<AppConfigBuilder> UseVersionMiddleware(string version)
        {
            _appVersion = version;
            return (AppConfigBuilder c) =>
            {
                c.UseMiddleware(DisplayVersionIfSpecified, MiddlewareSteps.Version);
                c.BuildEvents.OnCommandCreated += AddVersionOption;
            };
        }

        private static void AddVersionOption(CommandCreatedEventArgs args)
        {
            if (!args.CommandBuilder.Command.IsRootCommand()) return;
            if (args.CommandBuilder.Command.ContainsArgumentNode(VersionOptionName)) return;

            var option = new Option(VersionOptionName, 'v', TypeInfo.Flag, ArgumentArity.Zero,
                typeof(VersionMiddleware).FullName)
            {
                Description = "Show version information",
                IsMiddlewareOption = true
            };
            args.CommandBuilder.AddArgument(option);
        }

        private static Task<int> DisplayVersionIfSpecified(CommandContext commandContext,
            ExecutionDelegate next)
        {
            if (!commandContext.RootCommand!.HasInputValues(VersionOptionName)) return next(commandContext);
            Print(commandContext.Console);
            return ExitCodes.Success;
        }

        private static void Print(IConsole console)
        {
            console.Out.WriteLine(_appVersion);
        }
    }
}
