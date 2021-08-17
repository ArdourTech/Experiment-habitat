using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using ICSharpCode.SharpZipLib.Tar;
using static System.Convert;
using static System.IO.Directory;
using static System.IO.File;
using static System.IO.SearchOption;
using static Habitat.Cli.Utils.Objects;
using static Habitat.Cli.Utils.File;
using static ICSharpCode.SharpZipLib.Tar.TarEntry;

namespace Habitat.Cli.Utils
{
    public static class Zip
    {
        private static Func<string, bool> CancellationIsNotRequested(CancellationToken token = default) {
            return IsNull(token)
                ? ignored => true
                : ignored => !token.IsCancellationRequested;
        }

        public static Stream TarballDirectory(
            string directory,
            CancellationToken cancellationToken = default) {
            var stream = new MemoryStream();
            var files = GetFiles(directory, "*.*", AllDirectories)
                .Where(IsNotGitDirectory);
            Log.Debug("Creating Tar Archive...");
            using var archive = new TarOutputStream(stream, Encoding.UTF8) { IsStreamOwner = false };
            foreach (var file in files.TakeWhile(CancellationIsNotRequested(cancellationToken))) {
                var tarName = file[directory.Length..].Replace('\\', '/').TrimStart('/');
                Log.Debug($"\tAdding {tarName}");
                var entry = CreateTarEntry(tarName);
                using var fileStream = OpenRead(file);
                entry.Size = fileStream.Length;
                entry.TarHeader.Mode = ToInt32("100755", 8); //chmod 755
                archive.PutNextEntry(entry);
                var localBuffer = new byte[32 * 1024];
                while (true) {
                    var numRead = fileStream.Read(localBuffer, 0, localBuffer.Length);
                    if (numRead <= 0) break;
                    archive.Write(localBuffer, 0, numRead);
                }

                archive.CloseEntry();
            }

            archive.Close();
            stream.Position = 0;
            return stream;
        }
    }
}
