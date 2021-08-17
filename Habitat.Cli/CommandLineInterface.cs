using System.Threading.Tasks;
using CommandDotNet;
using CommandDotNet.Rendering;
using Habitat.Cli.Commands;

namespace Habitat.Cli
{
    public class CommandLineInterface
    {
        [SubCommand] public Build Build { get; set; }
        [SubCommand] public Connect Connect { get; set; }
        [SubCommand] public Start Start { get; set; }
        [SubCommand] public Stop Stop { get; set; }

        public Task<int> Interceptor(InterceptorExecutionDelegate next,
                                     IConsole console,
                                     VerbosityArgs verbosityArgs) {
            Log.Configure(verbosityArgs.Verbose, console);
            return next();
        }
    }
}
