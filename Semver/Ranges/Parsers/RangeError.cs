using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using Semver.Utility;

namespace Semver.Ranges.Parsers
{
    internal static class RangeError
    {
        private const string TooLongMessage = "Exceeded maximum length of {1} for '{0}'.";
        private const string InvalidOperatorMessage = "Invalid operator '{0}'.";
        private const string InvalidWhitespaceMessage
            = "Invalid whitespace character at {0} in '{1}'. Only the ASCII space character is allowed.";
        private const string MissingComparisonMessage
            = "Range is missing a comparison or limit at {0} in '{1}'.";
        private const string MaxVersionMessage
            = "Cannot construct range from version '{0}' because version number cannot be incremented beyond max value.";
        private const string WildcardWithOperatorMessage
            = "Operator is combined with wildcards in '{0}'.";
        private const string PrereleaseWithWildcardVersionMessage
            = "A wildcard major, minor, or patch is combined with a prerelease version in '{0}'.";
        private const string UnexpectedInHyphenRangeMessage
            = "Unexpected characters in hyphen range '{0}'.";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FormatException TooLong(string range, int maxLength)
            => NewFormatException(TooLongMessage, range.LimitLength(), maxLength);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Exception InvalidOperator(StringSegment @operator)
            => NewFormatException(InvalidOperatorMessage, @operator.ToStringLimitLength());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Exception InvalidWhitespace(int position, string range)
            => NewFormatException(InvalidWhitespaceMessage, position, range.LimitLength());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Exception MissingComparison(int position, string range)
            => NewFormatException(MissingComparisonMessage, position, range.LimitLength());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Exception MaxVersion(SemVersion version)
            => NewFormatException(MaxVersionMessage, version.ToString().LimitLength());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Exception WildcardNotSupportedWithOperator(string range)
            => NewFormatException(WildcardWithOperatorMessage, range.LimitLength());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Exception PrereleaseNotSupportedWithWildcardVersion(string range)
            => NewFormatException(PrereleaseWithWildcardVersionMessage, range.LimitLength());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Exception UnexpectedInHyphenRange(string unpexected)
            => NewFormatException(UnexpectedInHyphenRangeMessage, unpexected);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static FormatException NewFormatException(string messageTemplate, params object[] args)
            => new FormatException(string.Format(CultureInfo.InvariantCulture, messageTemplate, args));
    }
}
