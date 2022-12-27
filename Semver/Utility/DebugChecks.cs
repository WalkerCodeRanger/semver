using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Semver.Ranges;
using Semver.Ranges.Parsers;

namespace Semver.Utility
{
    /// <summary>
    /// The <see cref="DebugChecks"/> class allows for the various conditional checks done only in
    /// debug builds to not count against the code coverage metrics.
    /// </summary>
    /// <remarks>When using a preprocessor conditional block, the contained lines are not covered by
    /// the unit tests (see example below). This is expected because the conditions should not be
    /// reachable. But it makes it difficult to evaluate at a glance whether full code coverage has
    /// been reached.
    /// <code>
    /// #if DEBUG
    ///     if(condition) throw new Exception("...");
    /// #endif
    /// </code>
    /// </remarks>
    [ExcludeFromCodeCoverage]
    internal static class DebugChecks
    {
        [Conditional("DEBUG")]
        public static void IsValid(SemVersionRangeOptions rangeOptions, string paramName)
        {
            if (!rangeOptions.IsValid())
                throw new ArgumentException("DEBUG: " + SemVersionRange.InvalidOptionsMessage, paramName);
        }

        [Conditional("DEBUG")]
        public static void IsValidMaxLength(int maxLength, string paramName)
        {
            if (maxLength < 0)
                throw new ArgumentOutOfRangeException(paramName, "DEBUG: " + SemVersionRange.InvalidMaxLengthMessage);
        }

        [Conditional("DEBUG")]
        public static void IsNotWildcardVersionWithPrerelease(WildcardVersion wildcardVersion, SemVersion semver)
        {
            if (wildcardVersion != WildcardVersion.None && semver.IsPrerelease)
                throw new InvalidOperationException("DEBUG: prerelease not allowed with wildcard");
        }

        [Conditional("DEBUG")]
        public static void IsNotEmpty(StringSegment segment, string paramName)
        {
            if (segment.IsEmpty)
                throw new ArgumentException("DEBUG: Cannot be empty", paramName);
        }
    }
}
