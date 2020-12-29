using System;
using System.Runtime.CompilerServices;
using static Semver.SemVersionStyles;

namespace Semver
{
    internal static class SemVersionStylesExtensions
    {
        private const SemVersionStyles All = Any
                                             | DisallowMultiplePrereleaseIdentifiers
                                             | DisallowMetadata;
        private const SemVersionStyles OptionalMinorWithoutPatch = OptionalMinorPatch & ~OptionalPatch;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValid(this SemVersionStyles styles)
        {
            return (styles & All) == styles
                // Check for a flag for optional minor without optional patch
                && (styles & OptionalMinorPatch) != OptionalMinorWithoutPatch;
        }

        /// <summary>
        /// The <see cref="Enum.HasFlag"/> method is surprisingly slow. This provides
        /// a fast alternative for the <see cref="SemVersionStyles"/> enum.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasStyle(this SemVersionStyles styles, SemVersionStyles flag)
        {
            return (styles & flag) == flag;
        }
    }
}
