using System;

namespace Semver
{
    /// <summary>
    /// Options beyond the <see cref="SemVersionStyles"/> for version parsing used by range parsing.
    /// </summary>
    internal class SemVersionParsingOptions
    {
        /// <summary>
        /// No special parsing options. Used when parsing versions outside of ranges.
        /// </summary>
        public static SemVersionParsingOptions None
            = new SemVersionParsingOptions(false, false, _ => false);

        public SemVersionParsingOptions(
            bool allowWildcardMajorMinorPatch,
            bool allowWildcardPrerelease,
            Predicate<char> isWildcard)
        {
            AllowWildcardMajorMinorPatch = allowWildcardMajorMinorPatch;
            AllowWildcardPrerelease = allowWildcardPrerelease;
            IsWildcard = isWildcard;
        }

        public bool AllowWildcardMajorMinorPatch { get; }
        public bool AllowWildcardPrerelease { get; }
        public Predicate<char> IsWildcard { get; }
    }
}
