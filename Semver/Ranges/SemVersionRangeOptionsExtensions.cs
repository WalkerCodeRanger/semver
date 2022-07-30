using System;
using System.Runtime.CompilerServices;

namespace Semver.Ranges
{
    internal static class SemVersionRangeOptionsExtensions
    {
        /// <summary>
        /// The <see cref="Enum.HasFlag"/> method is surprisingly slow. This provides
        /// a fast alternative for the <see cref="SemVersionRangeOptions"/> enum.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasOption(this SemVersionRangeOptions options, SemVersionRangeOptions flag)
            => (options & flag) == flag;
    }
}
