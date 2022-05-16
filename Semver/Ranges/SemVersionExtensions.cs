using System;
using Semver.Ranges.Comparers.Npm;

namespace Semver.Ranges
{
    public static class SemVersionExtensions
    {
        /// <summary>
        /// <para>
        /// Checks if this version satisfies the specified range.
        /// Uses the same range syntax as npm.
        /// </para>
        /// <para>
        /// Note: It's more optimal to use the static parse methods on <see cref="NpmRange"/>
        /// if you're gonna be testing multiple versions against the same range
        /// to avoid having to parse the range multiple times.
        /// </para>
        /// </summary>
        /// <param name="version"></param>
        /// <param name="range">The range to compare with. If the syntax is invalid the method will always return false.</param>
        /// <param name="options">The options to use when parsing the range.</param>
        /// <returns>True if the version satisfies the range.</returns>
        /// <exception cref="ArgumentNullException">Thrown if version or range is null.</exception>
        public static bool SatisfiesNpm(this SemVersion version, string range, NpmParseOptions options = default)
        {
            if (version == null) throw new ArgumentNullException(nameof(version));
            if (range == null) throw new ArgumentNullException(nameof(range));
            if (!NpmRange.TryParse(range, options, out var parsedRange))
                return false;

            return parsedRange.Contains(version);
        }
        
        /// <summary>
        /// Checks if this version satisfies the specified range.
        /// Uses the same syntax as npm.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="range">The range to compare with.</param>
        /// <returns>True if the version satisfies the range.</returns>
        /// <exception cref="ArgumentNullException">Thrown if version or range is null.</exception>
        public static bool Satisfies(this SemVersion version, ISemVersionRange range)
        {
            if (version == null) throw new ArgumentNullException(nameof(version));
            if (range == null) throw new ArgumentNullException(nameof(range));

            return range.Contains(version);
        }
    }
}