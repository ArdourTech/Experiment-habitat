using System;
using System.Threading.Tasks;
using CommandDotNet;
using CommandDotNet.Execution;
using CommandDotNet.FluentValidation;
using CommandDotNet.NameCasing;

namespace Habitat.Cli
{
    public static class Program
    {
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
                .UseDefaultMiddleware()
                .UseNameCasing(Case.KebabCase)
                .UseDefaultsFromEnvVar()
                .UseFluentValidation()
                .Configure(BasicDependencyInjection)
                .RunAsync(args);
        }
    }
}