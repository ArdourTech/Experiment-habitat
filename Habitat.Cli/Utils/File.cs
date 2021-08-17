using System;
using System.IO;
using static System.Diagnostics.Debug;
using static System.IO.Path;
using static System.StringComparison;

namespace Habitat.Cli.Utils
{
    public static class File
    {
        public static bool IsNotGitDirectory(string v)
        {
            return !v.Contains(@"\.git\") && !v.EndsWith(@"\.git");
        }

        public static bool Exists(FileSystemInfo fsInfo)
        {
            return fsInfo.Exists;
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

        public static bool IsDescendant(FileInfo fileInfo, DirectoryInfo directoryInfo)
        {
            // https://connect.microsoft.com/VisualStudio/feedback/details/777308/inconsistent-behavior-of-fullname-when-provided-path-ends-with-a-backslash
            var path = directoryInfo.FullName.TrimEnd(DirectorySeparatorChar);
            var dir = fileInfo.Directory;
            while (dir != null)
            {
                if (dir.FullName.TrimEnd(DirectorySeparatorChar).Equals(path, OrdinalIgnoreCase)) return true;
                dir = dir.Parent;
            }

            return false;
        }
    }
}