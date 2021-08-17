using System.Threading.Tasks;
using CommandDotNet;
using CommandDotNet.Rendering;
using Habitat.Cli.Commands;

namespace Habitat.Cli
{
    public class CommandLineInterface
    {
        [SubCommand] public Build Build { get; set; } = null!;
        [SubCommand] public Connect Connect { get; set; } = null!;
        [SubCommand] public Start Start { get; set; } = null!;
        [SubCommand] public Stop Stop { get; set; } = null!;

        public Task<int> Interceptor(InterceptorExecutionDelegate next,
                                     IConsole console,
                                     VerbosityArgs verbosityArgs) {
            Log.Configure(verbosityArgs.Verbose, console);
            return next();
        }
    }
}
