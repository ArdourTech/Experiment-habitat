using System;
using System.IO;
using static System.Diagnostics.Debug;

namespace Habitat.Cli.Utils
{
    public static class File
    {
        public static bool IsNotGitDirectory(string v)
        {
            return !v.Contains(@"\.git\") && !v.EndsWith(@"\.git");
        }

        public static bool DirExists(DirectoryInfo directoryInfo)
        {
            return directoryInfo.Exists;
        }

        public static Func<DirectoryInfo, bool> DirContainsFile(string file)
        {
            Assert(file != null, nameof(file) + " != null");
            return directoryInfo =>
            {
                Assert(directoryInfo != null, nameof(directoryInfo) + " != null");
                return directoryInfo.GetFiles(file).Length == 1;
            };
        }
    }
}