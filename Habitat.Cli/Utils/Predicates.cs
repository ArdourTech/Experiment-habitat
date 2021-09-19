using System;

namespace DefaultNamespace
{
    public static class Predicates
    {
        public static Func<T, bool> Not<T>(Func<T, bool> containsKey) {
            return v => !containsKey.Invoke(v);
        }
    }
}
