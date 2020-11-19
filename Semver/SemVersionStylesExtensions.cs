using System;
using System.Runtime.CompilerServices;

namespace Semver
{
    internal static class SemVersionStylesExtensions
    {
        private const SemVersionStyles All = SemVersionStyles.Any
                                             | SemVersionStyles.DisallowMultiplePrereleaseIdentifiers
                                             | SemVersionStyles.DisallowMetadata;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValid(this SemVersionStyles styles)
        {
            return (styles & All) == styles;
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
