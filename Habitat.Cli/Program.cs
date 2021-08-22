using System;
using System.Threading.Tasks;
using CommandDotNet;
using CommandDotNet.FluentValidation;
using CommandDotNet.NameCasing;
using static Habitat.Cli.Builders.VersionMiddleware;

namespace Habitat.Cli
{
    public static class Program
    {
        private const string Version = "0.0.1";

        public static Task<int> Main(string[] args) {
            return new AppRunner<CommandLineInterface>()
                   .UseDefaultMiddleware(excludeVersionMiddleware: true)
                   .UseDefaultsFromEnvVar()
                   .UseNameCasing(Case.KebabCase)
                   .UseFluentValidation()
                   .UseErrorHandler(ErrorHandler!)
                   .Configure(UseVersionMiddleware(Version))
                   .Configure(WithParameterResolvers)
                   .RunAsync(args);
        }

        private static void WithParameterResolvers(AppConfigBuilder obj) {
            obj.UseParameterResolver(ctx => (IDocker)new Docker.Docker(ctx.CancellationToken));
        }

        private static int ErrorHandler(CommandContext ctx, Exception exception) {
            if (exception is TimeoutException) {
                Log.Error("Unexpected Timeout Exception. Is Docker running?");
            }
            else {
                Log.Error($"Unexpected Exception occured: {exception.GetType()}");
                Log.Error($"  {exception.Message}");
            }

            return 1;
        }
    }
}
