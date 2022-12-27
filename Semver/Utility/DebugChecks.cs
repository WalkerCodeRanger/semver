using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Semver.Ranges;
using Semver.Ranges.Parsers;

namespace Semver.Utility
{
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
    }
}
