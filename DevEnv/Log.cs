using CommandDotNet;
using CommandDotNet.Rendering;
using static System.String;
using static DevEnv.Utils.Objects;

namespace DevEnv
{
    public static class Log
    {
        private static IConsole _console;
        private static bool _isVerbose;

        public static void Configure(bool verbose, IConsole console = null)
        {
            _console = console;
            _isVerbose = verbose;
            Debug("Verbose Logging Enabled");
        }

        public static void Debug(string line)
        {
            if (_isVerbose) Info(line);
        }

        public static void Info(string line)
        {
            line = line.TrimEnd();
            if (NonNull(_console) && !IsNullOrWhiteSpace(line)) _console!.WriteLine(line);
        }

        public static void Error(string line)
        {
            line = line.TrimEnd();
            if (NonNull(_console) && !IsNullOrWhiteSpace(line)) _console!.Error.WriteLine(line);
        }
    }
}
