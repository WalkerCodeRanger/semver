using System;
using System.Collections.Generic;

namespace Semver.Benchmarks
{
    public static class Enumerables
    {
        public static IEnumerable<T> Generate<T>(int n, Func<T> generate)
        {
            for (int i = 0; i < n; i++)
                yield return generate();
        }
    }
}
