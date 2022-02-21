using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using Semver.Utility;

namespace Semver
{
    /// <summary>
    /// Parsing for <see cref="SemVersion"/>
    /// </summary>
    /// <remarks>The new parsing code was complex enough that is made sense to break out into its
    /// own class.</remarks>
    internal static class SemVersionParser
    {
        private const string LeadingWhitespaceMessage = "Version '{0}' has leading whitespace.";
        private const string TrailingWhitespaceMessage = "Version '{0}' has trailing whitespace.";
        private const string EmptyVersionMessage = "Empty string is not a valid version.";
        private const string TooLongVersionMessage = "Exceeded maximum length of {1} in '{0}'.";
        private const string AllWhitespaceVersionMessage = "Whitespace is not a valid version.";
        private const string LeadingLowerVMessage = "Leading 'v' in '{0}'.";
        private const string LeadingUpperVMessage = "Leading 'V' in '{0}'.";
        private const string LeadingZeroInMajorMinorOrPatchMessage = "{1} version has leading zero in '{0}'.";
        private const string EmptyMajorMinorOrPatchMessage = "{1} version missing in '{0}'.";
        private const string MissingMinorMessage = "Missing minor version in '{0}'.";
        private const string MissingPatchMessage = "Missing patch version in '{0}'.";
        private const string MajorMinorOrPatchOverflowMessage = "{1} version '{2}' was too large for Int32 in '{0}'.";
        private const string FourthVersionNumberMessage = "Fourth version number in '{0}'.";
        private const string PrereleasePrefixedByDotMessage = "The prerelease identfiers should be prefixed by '-' instead of '.' in '{0}'.";
        private const string MissingPrereleaseIdentifierMessage = "Missing prerelease identifier in '{0}'.";
        private const string LeadingZeroInPrereleaseMessage = "Leading zero in prerelease identifier in version '{0}'.";
        private const string PrereleaseOverflowMessage = "Prerelease identifier '{1}' was too large for Int32 in version '{0}'.";
        private const string InvalidCharacterInPrereleaseMessage = "Invalid character '{1}' in prerelease identifier in '{0}'.";
        private const string MissingMetadataIdentifierMessage = "Missing metadata identifier in '{0}'.";
        private const string InvalidCharacterInMajorMinorOrPatchMessage = "{1} version contains invalid character '{2}' in '{0}'.";
        private const string InvalidCharacterInMetadataMessage = "Invalid character '{1}' in metadata identifier in '{0}'.";

        /// <summary>
        /// The internal method that all parsing is based on. Because this is called by both
        /// <see cref="SemVersion.Parse(string, SemVersionStyles, int)"/> and
        /// <see cref="SemVersion.TryParse(string, SemVersionStyles, out SemVersion, int)"/>
        /// it does not throw exceptions, but instead returns the exception that should be thrown
        /// by the parse method. For performance when used from try parse, all exception construction
        /// and message formatting can be avoided by passing in an exception which will be returned
        /// when parsing fails.
        /// </summary>
        /// <remarks>This does not validate the <paramref name="style"/> or <paramref name="maxLength"/>
        /// parameter values. That must be done in the calling method.</remarks>
        public static Exception Parse(
            string version,
            SemVersionStyles style,
            Exception ex,
            int maxLength,
            out SemVersion semver)
        {
#if DEBUG
            if (!style.IsValid())
                throw new ArgumentException("DEBUG: " + SemVersion.InvalidSemVersionStylesMessage, nameof(style));
#endif

            // Assign null once so it doesn't have to be done any time parse fails
            semver = null;

            // Note: this method relies on the fact that the null coalescing operator `??`
            // is short circuiting to avoid constructing exceptions and exception messages
            // when a non-null exception is passed in.

            if (version is null) return ex ?? new ArgumentNullException(nameof(version));

            if (version.Length == 0) return ex ?? new FormatException(EmptyVersionMessage);

            if (version.Length > maxLength) return ex ?? NewFormatException(TooLongVersionMessage, LimitLength(version), maxLength);

            // This code does two things to help provide good error messages:
            // 1. It breaks the version number into segments and then parses those segments
            // 2. It parses an element first, then checks the flags for whether it should be allowed

            var i = 0;

            var parseEx = ParseLeadingWhitespace(version, style, ex, ref i);
            if (parseEx != null) return parseEx;

            parseEx = ParseLeadingV(version, style, ex, ref i);
            if (parseEx != null) return parseEx;

            // Now break the version number down into segments. Two segments have already been handled
            // namely leading whitespace and 'v' or 'V'.

            var startOfTrailingWhitespace = StartOfTrailingWhitespace(version);
            var startOfMetadata = version.IndexOf('+', i, startOfTrailingWhitespace - i);
            if (startOfMetadata < 0) startOfMetadata = startOfTrailingWhitespace;
            var startOfPrerelease = version.IndexOf('-', i, startOfMetadata - i);
            if (startOfPrerelease < 0) startOfPrerelease = startOfMetadata;

            // Are leading zeros allowed
            var allowLeadingZeros = style.HasStyle(SemVersionStyles.AllowLeadingZeros);

            // Parse major version
            parseEx = ParseVersionNumber("Major", version, ref i, startOfPrerelease, allowLeadingZeros, ex, out var major);
            if (parseEx != null) return parseEx;

            // Parse minor version
            var minor = 0;
            if (i < version.Length && version[i] == '.')
            {
                i += 1;
                parseEx = ParseVersionNumber("Minor", version, ref i, startOfPrerelease, allowLeadingZeros, ex, out minor);
                if (parseEx != null) return parseEx;
            }
            else if (!style.HasStyle(SemVersionStyles.OptionalMinorPatch))
                return ex ?? NewFormatException(MissingMinorMessage, LimitLength(version));

            // Parse patch version
            var patch = 0;
            if (i < version.Length && version[i] == '.')
            {
                i += 1;
                parseEx = ParseVersionNumber("Patch", version, ref i, startOfPrerelease, allowLeadingZeros, ex, out patch);
                if (parseEx != null) return parseEx;
            }
            else if (!style.HasStyle(SemVersionStyles.OptionalPatch))
                return ex ?? NewFormatException(MissingPatchMessage, LimitLength(version));

            // Handle fourth version number
            if (i < startOfPrerelease && version[i] == '.')
            {
                i += 1;
                // If it is ".\d" then we'll assume they were trying to have a fourth version number
                if (i < version.Length && version[i].IsDigit())
                    return ex ?? NewFormatException(FourthVersionNumberMessage, LimitLength(version));

                // Otherwise, assume they used "." instead of "-" to start the prerelease
                return ex ?? NewFormatException(PrereleasePrefixedByDotMessage, LimitLength(version));
            }

            // Parse prerelease version
            IReadOnlyList<PrereleaseIdentifier> prereleaseIdentifiers;
            if (i < version.Length && version[i] == '-')
            {
                i += 1;
                parseEx = ParsePrerelease(version, ref i, startOfMetadata, allowLeadingZeros, ex, out prereleaseIdentifiers);
                if (parseEx != null) return parseEx;
            }
            else
                prereleaseIdentifiers = ReadOnlyList<PrereleaseIdentifier>.Empty;

            // Parse metadata
            IReadOnlyList<MetadataIdentifier> metadataIdentifiers;
            if (i < version.Length && version[i] == '+')
            {
                i += 1;
                parseEx = ParseMetadata(version, ref i, startOfTrailingWhitespace, ex, out metadataIdentifiers);
                if (parseEx != null) return parseEx;
            }
            else
                metadataIdentifiers = ReadOnlyList<MetadataIdentifier>.Empty;

            // There shouldn't be any unprocessed characters before the trailing whitespace.
            // If there is, it is a programmer mistake, so immediately throw an exception rather
            // than returning an exception which try parse wouldn't throw.
            if (i != startOfTrailingWhitespace)
                throw new InvalidOperationException($"Error parsing '{version}'");

            // Error if trailing whitespace not allowed
            if (startOfTrailingWhitespace != version.Length && !style.HasStyle(SemVersionStyles.AllowTrailingWhitespace))
                return ex ?? NewFormatException(TrailingWhitespaceMessage, LimitLength(version));

            semver = new SemVersion(major, minor, patch, prereleaseIdentifiers, metadataIdentifiers);
            return null;
        }

        private static Exception ParseLeadingWhitespace(
            string version,
            SemVersionStyles style,
            Exception ex,
            ref int i)
        {
            // Skip leading whitespace
            while (i < version.Length && char.IsWhiteSpace(version, i)) i += 1;

            // Error if all whitespace
            if (i == version.Length)
                return ex ?? new FormatException(AllWhitespaceVersionMessage);

            // Error if leading whitespace not allowed
            if (i > 0 && !style.HasStyle(SemVersionStyles.AllowLeadingWhitespace))
                return ex ?? NewFormatException(LeadingWhitespaceMessage, LimitLength(version));

            return null;
        }

        private static Exception ParseLeadingV(string version, SemVersionStyles style, Exception ex, ref int i)
        {
            // This is safe because the check for all whitespace ensures there is at least one more char
            var leadChar = version[i];
            switch (leadChar)
            {
                case 'v' when style.HasStyle(SemVersionStyles.AllowLowerV):
                    i += 1;
                    break;
                case 'v':
                    return ex ?? NewFormatException(LeadingLowerVMessage, LimitLength(version));
                case 'V' when style.HasStyle(SemVersionStyles.AllowUpperV):
                    i += 1;
                    break;
                case 'V':
                    return ex ?? NewFormatException(LeadingUpperVMessage, LimitLength(version));
            }

            return null;
        }

        private static int StartOfTrailingWhitespace(string version)
        {
            var i = version.Length - 1;
            while (i > 0 && char.IsWhiteSpace(version, i)) i -= 1;
            return i + 1; // add one for the non-whitespace char that was found
        }

        private static Exception ParseVersionNumber(
            string kind,
            string version,
            ref int i,
            int startOfNext,
            bool allowLeadingZero,
            Exception ex,
            out int number)
        {
            var end = version.IndexOf('.', i, startOfNext - i);
            if (end < 0) end = startOfNext;

            var start = i;

            // Skip leading zeros
            while (i < end && version[i] == '0') i += 1;

            var startOfNonZeroDigits = i;

            while (i < end && version[i].IsDigit())
                i += 1;

            // If there are unprocessed characters, then it is an invalid char for this segment
            if (i < end)
            {
                number = 0;
                return ex ?? NewFormatException(InvalidCharacterInMajorMinorOrPatchMessage, LimitLength(version), kind, version[i]);
            }

            if (start == i)
            {
                number = 0;
                return ex ?? NewFormatException(EmptyMajorMinorOrPatchMessage, LimitLength(version), kind);
            }

            if (!allowLeadingZero)
            {
                // Since it isn't missing, if there are no non-zero digits, it must be zero
                var isZero = startOfNonZeroDigits == i;
                var maxLeadingZeros = isZero ? 1 : 0;
                if (startOfNonZeroDigits - start > maxLeadingZeros)
                {
                    number = 0;
                    return ex ?? NewFormatException(LeadingZeroInMajorMinorOrPatchMessage, LimitLength(version), kind);
                }
            }

            var numberString = version.Substring(start, i - start);
            if (!int.TryParse(numberString, NumberStyles.None, CultureInfo.InvariantCulture, out number))
                // Parsing validated this as a string of digits possibly proceeded by zero so the only
                // possible issue is a numeric overflow for `int`
                return ex ?? new OverflowException(string.Format(CultureInfo.InvariantCulture,
                    MajorMinorOrPatchOverflowMessage, LimitLength(version), kind, numberString));

            return null;
        }

        private static Exception ParsePrerelease(
            string version,
            ref int i,
            int startOfNext,
            bool allowLeadingZero,
            Exception ex,
            out IReadOnlyList<PrereleaseIdentifier> prereleaseIdentifiers)
        {
            var identifiers = new List<PrereleaseIdentifier>();
            prereleaseIdentifiers = identifiers.AsReadOnly();
            i -= 1; // Back up so we are before the start of the first identifier
            do
            {
                i += 1;
                var s = i;
                var isNumeric = true;
                while (i < startOfNext)
                {
                    var c = version[i];
                    if (c.IsAlphaOrHyphen())
                        isNumeric = false;
                    else if (c == '.' || c == '+')
                        break;
                    else if (!c.IsDigit())
                        return ex ?? NewFormatException(InvalidCharacterInPrereleaseMessage, LimitLength(version), c);

                    i += 1;
                }

                // Empty identifiers not allowed
                if (s == i)
                    return ex ?? NewFormatException(MissingPrereleaseIdentifierMessage, LimitLength(version));

                var identifier = version.Substring(s, i - s);
                if (!isNumeric)
                    identifiers.Add(PrereleaseIdentifier.CreateUnsafe(identifier, null));
                else
                {
                    if (!allowLeadingZero && version[s] == '0')
                        return ex ?? NewFormatException(LeadingZeroInPrereleaseMessage, LimitLength(version));

                    if (!int.TryParse(identifier, NumberStyles.None, null, out var intValue))
                        // Parsing validated this as a string of digits possibly proceeded by zero so the only
                        // possible issue is a numeric overflow for `int`
                        return ex ?? new OverflowException(string.Format(CultureInfo.InvariantCulture,
                            PrereleaseOverflowMessage, LimitLength(version), identifier));

                    identifiers.Add(PrereleaseIdentifier.CreateUnsafe(identifier.TrimStart('0'), intValue));
                }

            } while (i < startOfNext && version[i] == '.');

            return null;
        }

        private static Exception ParseMetadata(
            string version,
            ref int i,
            int startOfNext,
            Exception ex,
            out IReadOnlyList<MetadataIdentifier> metadataIdentifiers)
        {
            var identifiers = new List<MetadataIdentifier>();
            metadataIdentifiers = identifiers.AsReadOnly();
            i -= 1; // Back up so we are before the start of the first identifier
            do
            {
                i += 1; // Advance to start of identifier
                var s = i;
                while (i < startOfNext)
                {
                    var c = version[i];
                    if (c == '.')
                        break;
                    if (!c.IsAlphaOrHyphen() && !c.IsDigit())
                        return ex ?? NewFormatException(InvalidCharacterInMetadataMessage, LimitLength(version), c);
                    i += 1;
                }

                // Empty identifiers not allowed
                if (s == i)
                    return ex ?? NewFormatException(MissingMetadataIdentifierMessage, LimitLength(version));

                var identifier = version.Substring(s, i - s);
                identifiers.Add(MetadataIdentifier.CreateUnsafe(identifier));

            } while (i < startOfNext && version[i] == '.');

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static FormatException NewFormatException(string messageTemplate, params object[] args)
            => new FormatException(string.Format(CultureInfo.InvariantCulture, messageTemplate, args));

        private const int VersionDisplayLimit = 100;

        private static string LimitLength(string version)
        {
            if (version.Length > VersionDisplayLimit)
                version = version.Substring(0, VersionDisplayLimit - 3) + "...";

            return version;
        }
    }
}
