using System;
using System.Runtime.CompilerServices;
using static Semver.Ranges.SemVersionRangeOptions;

namespace Semver.Ranges
{
    internal static class SemVersionRangeOptionsExtensions
    {
        private const SemVersionRangeOptions OptionsThatAreStyles
            = AllowLeadingZeros | AllowV | OptionalMinorPatch;
        /// <summary>
        /// The <see cref="Enum.HasFlag"/> method is surprisingly slow. This provides
        /// a fast alternative for the <see cref="SemVersionRangeOptions"/> enum.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasOption(this SemVersionRangeOptions options, SemVersionRangeOptions flag)
            => (options & flag) == flag;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SemVersionStyles ToStyles(this SemVersionRangeOptions options)
            => (SemVersionStyles)(options & OptionsThatAreStyles);
    }
}
