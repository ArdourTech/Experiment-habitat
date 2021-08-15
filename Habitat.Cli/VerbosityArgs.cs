using CommandDotNet;

namespace Habitat.Cli
{
    public class VerbosityArgs : IArgumentModel
    {
        [Option(
            LongName = "verbose",
            Description = "Enable debug level logging throughout command execution")]
        public bool Verbose { get; set; }
    }
}