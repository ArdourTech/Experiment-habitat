namespace DevEnv.Utils
{
    public static class Objects
    {
        public static bool NonNull(object v)
        {
            return !IsNull(v);
        }

        public static bool IsNull(object v)
        {
            return v == null;
        }
    }
}