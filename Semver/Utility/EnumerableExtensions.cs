using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Semver.Utility
{
    internal static class EnumerableExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IReadOnlyList<T> ToReadOnlyList<T>(this IEnumerable<T> values)
            => values.ToList().AsReadOnly();

#if !NETSTANDARD1_6_OR_GREATER && !NET471_OR_GREATER && !NETCOREAPP
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<TSource> Prepend<TSource>(this IEnumerable<TSource> source, TSource element)
            => new[] { element }.Concat(source);
#endif
    }
}
