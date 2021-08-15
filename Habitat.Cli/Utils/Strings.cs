namespace Habitat.Cli.Utils
{
    public static class Strings
    {
        public static bool IsBlank(string v)
        {
            return string.IsNullOrEmpty(v);
        }

        public static bool IsNotBlank(string v)
        {
            return !IsBlank(v);
        }
    }
}