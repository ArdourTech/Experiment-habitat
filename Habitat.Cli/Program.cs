using System;
using System.Threading.Tasks;
using CommandDotNet;
using CommandDotNet.Execution;
using CommandDotNet.FluentValidation;
using CommandDotNet.NameCasing;
using static Habitat.Cli.Builders.VersionMiddleware;

namespace Habitat.Cli
{
    public static class Program
    {
        private const string Version = "0.0.1";

        private static readonly Action<AppConfigBuilder> BasicDependencyInjection = b => b
            .UseMiddleware(PoorManDependencyInjection, MiddlewareStages.BindValues);

        private static Task<int> PoorManDependencyInjection(CommandContext context, ExecutionDelegate next)
        {
            context.Services.Add(typeof(IDocker), new Docker());
            return next(context);
        }

        public static Task<int> Main(string[] args)
        {
            return new AppRunner<CommandLineInterface>()
                .Configure(UseVersionMiddleware(Version))
                .UseDefaultMiddleware()
                .UseNameCasing(Case.KebabCase)
                .UseDefaultsFromEnvVar()
                .UseFluentValidation()
                .Configure(BasicDependencyInjection)
                .UseErrorHandler(ErrorHandler)
                .RunAsync(args);
        }

        private static int ErrorHandler(CommandContext ctx, Exception exception)
        {
            if (exception is TimeoutException)
            {
                Log.Error("Unexpected Timeout Exception. Is Docker running?");
            }
            else
            {
                Log.Error($"Unexpected Exception occured: {exception.GetType()}");
                Log.Error($"  {exception.Message}");
            }

            return ExitCodes.Error.Result;
        }
    }
}