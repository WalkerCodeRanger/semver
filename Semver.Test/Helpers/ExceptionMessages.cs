using System;

namespace Semver.Test.Helpers
{
    public static class ExceptionMessages
    {
        /// <summary>
        /// Default exception message for <see cref="ArgumentNullException"/>.
        /// </summary>
        /// <remarks>It might be thought it is not necessary to test for this message. However,
        /// the Semver package internally has debug exception checking and the tests need to verify
        /// that an <see cref="ArgumentNullException"/> will be included in the release rather than
        /// is just coming from the debug exception checking.</remarks>
        public const string NotNull = "Value cannot be null.";

        public const string NoMetadata = "Cannot have metadata.";

        public const string InvalidSemVersionStylesStart = "An invalid SemVersionStyles value was used.";

        #region Parsing SemVersion
        public const string LeadingWhitespace = "Version '{version}' has leading whitespace.";
        public const string TrailingWhitespace = "Version '{version}' has trailing whitespace.";
        public const string EmptyVersion = "Empty string is not a valid version.";
        public const string TooLongVersion = "Exceeded maximum length of {value} for '{version}'.";
        public const string AllWhitespaceVersion = "Whitespace is not a valid version.";
        public const string LeadingLowerV = "Leading 'v' in '{version}'.";
        public const string LeadingUpperV = "Leading 'V' in '{version}'.";
        public const string LeadingZeroInMajor = "Major version has leading zero in '{version}'.";
        public const string LeadingZeroInMinor = "Minor version has leading zero in '{version}'.";
        public const string LeadingZeroInPatch = "Patch version has leading zero in '{version}'.";
        public const string EmptyMajor = "Major version missing in '{version}'.";
        public const string EmptyMinor = "Minor version missing in '{version}'.";
        public const string EmptyPatch = "Patch version missing in '{version}'.";
        public const string MissingMinor = "Minor version missing in '{version}'.";
        public const string MissingPatch = "Patch version missing in '{version}'.";
        public const string MajorOverflow = "Major version '{value}' was too large for Int32 in '{version}'.";
        public const string MinorOverflow = "Minor version '{value}' was too large for Int32 in '{version}'.";
        public const string PatchOverflow = "Patch version '{value}' was too large for Int32 in '{version}'.";
        public const string FourthVersionNumber = "Fourth version number in '{version}'.";
        public const string PrereleasePrefixedByDot = "The prerelease identfiers should be prefixed by '-' instead of '.' in '{version}'.";
        public const string MissingPrereleaseIdentifier = "Missing prerelease identifier in '{version}'.";
        public const string LeadingZeroInPrerelease = "Leading zero in prerelease identifier in version '{version}'.";
        public const string PrereleaseOverflow = "Prerelease identifier '{value}' was too large for Int32 in version '{version}'.";
        public const string InvalidCharacterInPrerelease = "Invalid character '{value}' in prerelease identifier in '{version}'.";
        public const string MissingMetadataIdentifier = "Missing metadata identifier in '{version}'.";
        public const string InvalidCharacterInMajor = "Major version contains invalid character '{value}' in '{version}'.";
        public const string InvalidCharacterInMinor = "Minor version contains invalid character '{value}' in '{version}'.";
        public const string InvalidCharacterInPatch = "Patch version contains invalid character '{value}' in '{version}'.";
        public const string InvalidCharacterInMetadata = "Invalid character '{value}' in metadata identifier in '{version}'.";
        #endregion

        #region Parsing Ranges
        public const string InvalidSemVersionRangeOptionsStart
            = "An invalid SemVersionRangeOptions value was used.";
        public const string InvalidMaxLengthStart = "Must not be negative.";
        public const string TooLongRange = "Exceeded maximum length of {value} for '{range}'.";
        public const string InvalidOperator = "Invalid operator '{value}'.";
        public const string InvalidWhitespace
            = "Invalid whitespace character at {value} in '{range}'. Only the ASCII space character is allowed.";
        public const string MissingComparison
            = "Range is missing a comparison or limit at {value} in '{range}'.";
        public const string MaxVersion
            = "Cannot construct range because version number cannot be incremented beyond max value in '{version}'.";
        public const string InvalidWildcardInPrerelease
            = "Prerelease version is a wildcard and should contain only 1 character in '{range}'.";
        public const string PrereleaseWildcardMustBeLast
            = "Prerelease identifier follows wildcard prerelease identifier in '{range}'.";
        public const string PrereleaseWithWildcardVersion
            = "A wildcard major, minor, or patch is combined with a prerelease version in '{range}'.";
        public const string WildcardNotSupportedWithOperator
            = "Operator is combined with wildcards in '{range}'.";
        #endregion

        public static string InjectValue(string format, string value)
            => format.Replace("{value}", value);

        public static string InjectVersion(string format, string version)
            => format.Replace("{version}", version.LimitLength());

        public static string InjectRange(string format, string range)
            => format.Replace("{range}", range.LimitLength());
    }
}
