using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Semver
{
    /// <summary>
    /// Parsing for <see cref="SemVersion"/>
    /// </summary>
    /// <remarks>The new parsing code was complex enough that is made sense to break out into its
    /// own class.</remarks>
    internal static class SemVersionParser
    {
        private const string LeadingWhitespaceMessage = "Version '{0}' has leading whitespace";
        private const string EmptyVersionMessage = "Empty string";
        private const string AllWhitespaceVersionMessage = "All whitespace";
        private const string LeadingLowerVMessage = "Leading 'v' in '{0}'";
        private const string LeadingUpperVMessage = "Leading 'V' in '{0}'";
        private const string LeadingZeroInMajorMinorOrPatchMessage = "Leading Zero in major, minor, or patch version in '{0}'";
        private const string MissingMajorMinorOrPatchMessage = "Missing major, minor, or patch version in '{0}'";
        private const string MissingMinorMessage = "Missing minor version in '{0}'";
        private const string MissingPatchMessage = "Missing patch version in '{0}'";
        private const string MajorMinorOrPatchOverflowMessage = "Major, minor, or patch version '{1}' was too large for Int32 in '{0}'";
        private const string FourthVersionNumberMessage = "Fourth version number in '{0}'";
        private const string PrereleasePrefixedByDotMessage = "The prerelease identfiers should be prefixed by '-' instead of '.' in '{0}'";
        private const string MissingPrereleaseIdentifierMessage = "Missing prerelease identifier in '{0}'";
        private const string LeadingZeroInPrereleaseMessage = "Leading Zero in prerelease identifier in version '{0}'";
        private const string PrereleaseOverflowMessage = "Prerelease identifier '{1}' was too large for Int32 in version '{0}'";
        private const string InvalidCharacterInPrereleaseMessage = "Invalid character '{1}' in prerelease identifier in '{0}'";
        private const string MissingMetadataIdentifierMessage = "Missing metadata identifier in '{0}'";
        private const string InvalidCharacterInMetadataMessage = "Invalid character '{1}' in metadata identifier in '{0}'";
        private const string InvalidCharacterInMajorMinorOrPatchMessage = "Invalid character '{1}' in major, minor, or patch version in '{0}'";


        /// <summary>
        /// The internal method that all parsing is based on. Because this is called by both
        /// <see cref="SemVersion.Parse(string, SemVersionStyles)"/> and
        /// <see cref="SemVersion.TryParse(string, SemVersionStyles, out SemVersion)"/>
        /// it does not throw exceptions, but instead returns the exception that should be thrown
        /// by the parse method. For performance when used from try parse, all exception construction
        /// and message formatting can be avoided by passing in an exception which will be returned
        /// when parsing fails.
        /// </summary>
        /// <remarks>This does not validate the <paramref name="style"/> parameter.
        /// That must be done in the calling method.</remarks>
        public static Exception Parse(
            string version,
            SemVersionStyles style,
            Exception ex,
            out SemVersion semver)
        {
            // Assign null once so it doesn't have to be done any time parse fails
            semver = null;

            // Note: this method relies on the fact that the null coalescing operator `??`
            // is short circuiting to avoid constructing exceptions and exception messages
            // when a non-null exception is passed in.

            if (version is null) return ex ?? new ArgumentNullException(nameof(version));

            if (version.Length == 0) return ex ?? new FormatException(EmptyVersionMessage);

            // To provide good error messages, this code always parses an element
            // first and then checks whether it should be allowed

            var i = 0;

            // Skip leading whitespace
            while (i < version.Length && char.IsWhiteSpace(version, i)) i += 1;

            // Error if all whitespace
            if (i == version.Length)
                return ex ?? new FormatException(AllWhitespaceVersionMessage);

            // Error if leading whitespace not allowed
            if (i > 0 && style.HasStyle(SemVersionStyles.AllowLeadingWhitespace))
                return ex ?? NewFormatException(LeadingWhitespaceMessage, version);

            // Handle leading 'v' or 'V'
            var leadChar = version[i]; // Safe because length checked above for all whitespace error
            if (leadChar == 'v')
            {
                if (style.HasStyle(SemVersionStyles.AllowLowerV)) i += 1;
                else return ex ?? NewFormatException(LeadingLowerVMessage, version);
            }
            else if (leadChar == 'V')
            {
                if (style.HasStyle(SemVersionStyles.AllowUpperV)) i += 1;
                else return ex ?? NewFormatException(LeadingUpperVMessage, version);
            }

            // Are leading zeros allowed
            var allowLeadingZeros = style.HasStyle(SemVersionStyles.AllowLeadingZeros);

            // Parse major version
            var parseEx = ParseMajorMinorOrPatch(version, ref i, allowLeadingZeros, ex, out var major);
            if (parseEx != null) return parseEx;

            // Parse minor version
            var minor = 0;
            if (i < version.Length && version[i] == '.')
            {
                i += 1;
                parseEx = ParseMajorMinorOrPatch(version, ref i, allowLeadingZeros, ex, out minor);
                if (parseEx != null) return parseEx;
            }
            else if (!style.HasStyle(SemVersionStyles.OptionalMinorPatch))
                return ex ?? NewFormatException(MissingMinorMessage, version);

            // Parse patch version
            var patch = 0;
            if (i < version.Length && version[i] == '.')
            {
                i += 1;
                parseEx = ParseMajorMinorOrPatch(version, ref i, allowLeadingZeros, ex, out patch);
                if (parseEx != null) return parseEx;
            }
            else if (!style.HasStyle(SemVersionStyles.OptionalPatch))
                return ex ?? NewFormatException(MissingPatchMessage, version);

            // Handle fourth version number
            if (i < version.Length && version[i] == '.')
            {
                i += 1;
                if (i < version.Length)
                {
                    if (version[i].IsDigit())
                        return ex ?? NewFormatException(FourthVersionNumberMessage, version);
                    return ex ?? NewFormatException(PrereleasePrefixedByDotMessage, version);
                }
                // There is just an extra dot at the end
                return ex ?? NewFormatException(InvalidCharacterInMajorMinorOrPatchMessage, version, '.');
            }

            // Parse prerelease version
            var allowMultiplePrereleaseIdentifiers = !style.HasStyle(SemVersionStyles.DisallowMultiplePrereleaseIdentifiers);
            List<PrereleaseIdentifier> prereleaseIdentifiers;
            if (i < version.Length && version[i] == '-')
            {
                i += 1;
                parseEx = ParsePrerelease(version, ref i, allowLeadingZeros, allowMultiplePrereleaseIdentifiers, ex, out prereleaseIdentifiers);
                if (parseEx != null) return parseEx;
            }
            else
                prereleaseIdentifiers = new List<PrereleaseIdentifier>();

            // Parse metadata
            var allowMetadata = !style.HasStyle(SemVersionStyles.DisallowMetadata);
            List<string> metadataIdentifiers;
            if (allowMetadata && i < version.Length && version[i] == '+')
            {
                i += 1;
                parseEx = ParseMetadata(version, ref i, ex, out metadataIdentifiers);
                if (parseEx != null) return parseEx;
            }
            else
                metadataIdentifiers = new List<string>();

            // Deal with unprocessed characters
            if (i != version.Length)
            {
                if (ex != null) return ex;
                var invalidChar = version[i];
                // Determine where to report the invalid character
                if (metadataIdentifiers.Count > 0)
                    return NewFormatException(InvalidCharacterInMetadataMessage, version, invalidChar);

                if (prereleaseIdentifiers.Count > 0)
                    return NewFormatException(InvalidCharacterInPrereleaseMessage, version, invalidChar);

                return NewFormatException(InvalidCharacterInMajorMinorOrPatchMessage, version, invalidChar);
            }

            semver = new SemVersion(major, minor, patch,
                new ReadOnlyCollection<PrereleaseIdentifier>(prereleaseIdentifiers),
                new ReadOnlyCollection<string>(metadataIdentifiers));
            return null;
        }

        private static Exception ParseMajorMinorOrPatch(
            string version,
            ref int i,
            bool allowLeadingZero,
            Exception ex,
            out int number)
        {
            var start = i;

            // Skip leading zero
            while (i < version.Length && version[i] == '0') i += 1;

            var startOfNonZeroDigits = i;

            while (i < version.Length && version[i].IsDigit())
                i += 1;

            if (start == i)
            {
                number = 0;
                return ex ?? NewFormatException(MissingMajorMinorOrPatchMessage, version);
            }

            if (!allowLeadingZero)
            {
                // Since it isn't missing, if there are no non-zero digits, it must be zero
                var isZero = startOfNonZeroDigits == i;
                var maxLeadingZeros = isZero ? 1 : 0;
                if (startOfNonZeroDigits - start > maxLeadingZeros)
                {
                    number = 0;
                    return ex ?? NewFormatException(LeadingZeroInMajorMinorOrPatchMessage, version);
                }
            }

            var numberString = version.Substring(start, i - start);
            if (!int.TryParse(numberString, NumberStyles.None, CultureInfo.InvariantCulture, out number))
                // Parsing validated this as a string of digits possibly proceeded by zero so the only
                // possible issue is a numeric overflow for `int`
                return ex ?? new OverflowException(string.Format(CultureInfo.InvariantCulture,
                    MajorMinorOrPatchOverflowMessage, version, numberString));

            return null;
        }

        private static Exception ParsePrerelease(
            string version,
            ref int i,
            bool allowLeadingZero,
            bool allowMultiplePrereleaseIdentifiers,
            Exception ex,
            out List<PrereleaseIdentifier> prereleaseIdentifiers)
        {
            prereleaseIdentifiers = new List<PrereleaseIdentifier>();
            i -= 1; // Back up so we are before the start of the first identifier
            do
            {
                i += 1;
                var s = i;
                var isNumeric = true;
                while (i < version.Length)
                {
                    var c = version[i];
                    if (c.IsAlphaOrHyphen())
                        isNumeric = false;
                    else if (c=='.' || c =='+')
                        break;
                    else if (!c.IsDigit())
                        return ex ?? NewFormatException(InvalidCharacterInPrereleaseMessage, version, c);

                    i += 1;
                }

                // Empty identifiers not allowed
                if (s == i)
                    return ex ?? NewFormatException(MissingPrereleaseIdentifierMessage, version);

                var identifier = version.Substring(s, i - s);
                if (!isNumeric)
                    prereleaseIdentifiers.Add(new PrereleaseIdentifier(identifier, null));
                else
                {
                    if (!allowLeadingZero && version[s] == '0')
                        return ex ?? NewFormatException(LeadingZeroInPrereleaseMessage, version);

                    if (!int.TryParse(identifier, NumberStyles.None, null, out var intValue))
                        return ex ?? NewFormatException(PrereleaseOverflowMessage, version, identifier);

                    prereleaseIdentifiers.Add(new PrereleaseIdentifier(identifier.TrimStart('0'), intValue));
                }

            } while (i < version.Length && version[i] == '.' && allowMultiplePrereleaseIdentifiers);

            return null;
        }

        private static Exception ParseMetadata(
            string version,
            ref int i,
            Exception ex,
            out List<string> metadataIdentifiers)
        {
            metadataIdentifiers = new List<string>();
            i -= 1; // Back up so we are before the start of the first identifier
            do
            {
                i += 1; // Advance to start of identifier
                var s = i;
                while (i < version.Length)
                {
                    var c = version[i];
                    if (c == '.')
                        break;
                    if (!c.IsAlphaOrHyphen() && !c.IsDigit())
                        return ex ?? NewFormatException(InvalidCharacterInMetadataMessage, version, c);
                    i += 1;
                }

                // Empty identifiers not allowed
                if (s == i)
                    return ex ?? NewFormatException(MissingMetadataIdentifierMessage, version);

                var identifier = version.Substring(s, i - s);
                metadataIdentifiers.Add(new PrereleaseIdentifier(identifier, null));

            } while (i < version.Length && version[i] == '.');

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static FormatException NewFormatException(string messageTemplate, string version)
        {
            return new FormatException(string.Format(CultureInfo.InvariantCulture, messageTemplate, version));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static FormatException NewFormatException(string messageTemplate, string version, string value)
        {
            return new FormatException(string.Format(CultureInfo.InvariantCulture, messageTemplate, version, value));
        }

        /// <remarks>This overload avoids issues with culture when trying to convert the char to a
        /// string at the call site.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static FormatException NewFormatException(string messageTemplate, string version, char value)
        {
            return new FormatException(string.Format(CultureInfo.InvariantCulture, messageTemplate, version, value));
        }
    }
}
