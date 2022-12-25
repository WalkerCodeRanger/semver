using System;
using System.Globalization;

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
        public const string LeadingWhitespace = "Version '{0}' has leading whitespace.";
        public const string TrailingWhitespace = "Version '{0}' has trailing whitespace.";
        public const string EmptyVersion = "Empty string is not a valid version.";
        public const string TooLongVersion = "Exceeded maximum length of {1} for '{0}'.";
        public const string AllWhitespaceVersion = "Whitespace is not a valid version.";
        public const string LeadingLowerV = "Leading 'v' in '{0}'.";
        public const string LeadingUpperV = "Leading 'V' in '{0}'.";
        public const string LeadingZeroInMajor = "Major version has leading zero in '{0}'.";
        public const string LeadingZeroInMinor = "Minor version has leading zero in '{0}'.";
        public const string LeadingZeroInPatch = "Patch version has leading zero in '{0}'.";
        public const string EmptyMajor = "Major version missing in '{0}'.";
        public const string EmptyMinor = "Minor version missing in '{0}'.";
        public const string EmptyPatch = "Patch version missing in '{0}'.";
        public const string MissingMinor = "Minor version missing in '{0}'.";
        public const string MissingPatch = "Patch version missing in '{0}'.";
        public const string MajorOverflow = "Major version '{1}' was too large for Int32 in '{0}'.";
        public const string MinorOverflow = "Minor version '{1}' was too large for Int32 in '{0}'.";
        public const string PatchOverflow = "Patch version '{1}' was too large for Int32 in '{0}'.";
        public const string FourthVersionNumber = "Fourth version number in '{0}'.";
        public const string PrereleasePrefixedByDot = "The prerelease identfiers should be prefixed by '-' instead of '.' in '{0}'.";
        public const string MissingPrereleaseIdentifier = "Missing prerelease identifier in '{0}'.";
        public const string LeadingZeroInPrerelease = "Leading zero in prerelease identifier in version '{0}'.";
        public const string PrereleaseOverflow = "Prerelease identifier '{1}' was too large for Int32 in version '{0}'.";
        public const string InvalidCharacterInPrerelease = "Invalid character '{1}' in prerelease identifier in '{0}'.";
        public const string MissingMetadataIdentifier = "Missing metadata identifier in '{0}'.";
        public const string InvalidCharacterInMajor = "Major version contains invalid character '{1}' in '{0}'.";
        public const string InvalidCharacterInMinor = "Minor version contains invalid character '{1}' in '{0}'.";
        public const string InvalidCharacterInPatch = "Patch version contains invalid character '{1}' in '{0}'.";
        public const string InvalidCharacterInMetadata = "Invalid character '{1}' in metadata identifier in '{0}'.";
        #endregion

        public static string InjectValue(string format, string value)
        {
            try
            {
                return string.Format(CultureInfo.InvariantCulture, format, "{0}", value);
            }
            catch (FormatException ex)
            {
                throw new FormatException($"Could not inject '{value}' into '{format}'", ex);
            }
        }
    }
}
