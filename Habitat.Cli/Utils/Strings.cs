using static System.String;

namespace Habitat.Cli.Utils
{
    public static class Strings
    {
        public static bool IsBlank(string? v) {
            return IsNullOrEmpty(v);
        }

        public static bool IsNotBlank(string? v) {
            return !IsBlank(v);
        }
    }
}
