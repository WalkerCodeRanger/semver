using System.Collections.Generic;

namespace Semver.Test.Helpers
{
    public static class ReadOnlyListExtensions
    {
        public static IEnumerable<(T, T)> AllPairs<T>(this IReadOnlyList<T> values)
        {
            for (var i = 0; i < values.Count; i++)
                for (var j = i + 1; j < values.Count; j++)
                    yield return (values[i], values[j]);
        }
    }
}
