using System.Collections.Generic;
using System.Linq;

namespace Semver.Test.Helpers;

public static class EnumerableExtensions
{
#if !NET6_0_OR_GREATER
    public static TSource? Max<TSource>(this IEnumerable<TSource> source, IComparer<TSource> comparer)
        => source.OrderByDescending(x => x, comparer).FirstOrDefault();
#endif
}
