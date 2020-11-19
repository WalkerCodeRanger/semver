using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
#if !NETSTANDARD
using System.Runtime.Serialization;
using System.Security.Permissions;
#endif
using System.Text.RegularExpressions;

namespace Semver
{
    /// <summary>
    /// A semantic version implementation.
    /// Conforms with v2.0.0 of http://semver.org
    /// </summary>
#if NETSTANDARD
    public sealed class SemVersion : IComparable<SemVersion>, IComparable
#else
    [Serializable]
    public sealed class SemVersion : IComparable<SemVersion>, IComparable, ISerializable
#endif
    {
        private const string InvalidSemVersionStylesMessage = "An invalid SemVersionStyles value was used.";
        private const string LeadingWhitespaceMessage = "Version '{0}' has leading whitespace";
        private const string EmptyVersionMessage = "Empty string";
        private const string AllWhitespaceVersionMessage = "All whitespace";
        private const string LeadingLowerVMessage = "Leading Lower V {0}";
        private const string LeadingUpperVMessage = "Leading Upper V {0}";
        private const string LeadingZeroMajorMinorPatchMessage = "Leading Zero in major, minor, patch {0}";
        private const string MissingMajorMinorPatchAfterDotMessage = "Missing major, minor, patch {0}";
        private const string NumberParseInvalid = "Number parse is invalid {0}";
        private const string MinorVersionNotOptionalMessage = "MinorVersionNotOptional {0}";
        private const string PatchVersionNotOptionalMessage = "PatchVersionNotOptional {0}";

        /// <summary>
        /// This exception is used with the <see cref="ParseVersion"/>
        /// method to indicate parse failure without constructing a new exception.
        /// This exception should never be thrown or exposed outside of this
        /// package.
        /// </summary>
        private static readonly Exception ParseFailedException = new Exception("Parse Failed");

        private static readonly Regex ParseEx =
            new Regex(@"^(?<major>\d+)" +
                @"(?>\.(?<minor>\d+))?" +
                @"(?>\.(?<patch>\d+))?" +
                @"(?>\-(?<pre>[0-9A-Za-z\-\.]+))?" +
                @"(?>\+(?<metadata>[0-9A-Za-z\-\.]+))?$",
#if NETSTANDARD
                RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture,
#else
                RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.ExplicitCapture,
#endif
                TimeSpan.FromSeconds(0.5));

#if !NETSTANDARD
#pragma warning disable CA1801 // Parameter unused
        /// <summary>
        /// Deserialize a <see cref="SemVersion"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">The <paramref name="info"/> parameter is null.</exception>
        private SemVersion(SerializationInfo info, StreamingContext context)
#pragma warning restore CA1801 // Parameter unused
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            var semVersion = Parse(info.GetString("SemVersion"));
            Major = semVersion.Major;
            Minor = semVersion.Minor;
            Patch = semVersion.Patch;
            Prerelease = semVersion.Prerelease;
            Metadata = semVersion.Metadata;
        }
#endif

        /// <summary>
        /// Constructs a new instance of the <see cref="SemVersion" /> class.
        /// </summary>
        /// <param name="major">The major version.</param>
        /// <param name="minor">The minor version.</param>
        /// <param name="patch">The patch version.</param>
        /// <param name="prerelease">The prerelease version (e.g. "alpha").</param>
        /// <param name="build">The build metadata (e.g. "nightly.232").</param>
        public SemVersion(int major, int minor = 0, int patch = 0, string prerelease = "", string build = "")
        {
            Major = major;
            Minor = minor;
            Patch = patch;

            Prerelease = prerelease ?? "";
            PrereleaseIdentifiers = new ReadOnlyCollection<PrereleaseIdentifier>(
                Prerelease.SplitExceptEmpty('.').Select(identifier => int.TryParse(identifier, NumberStyles.None, null, out var intValue)
                    ? new PrereleaseIdentifier(identifier, intValue)
                    : new PrereleaseIdentifier(identifier, null)).ToList());
            Metadata = build ?? "";
            MetadataIdentifiers = new ReadOnlyCollection<string>(Metadata.SplitExceptEmpty('.').ToList());
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="SemVersion"/> class from
        /// a <see cref="System.Version"/>.
        /// </summary>
        /// <param name="version">The <see cref="Version"/> that is used to initialize
        /// the Major, Minor, and Patch versions and the build metadata.</param>
        /// <returns>A <see cref="SemVersion"/> with the same Major and Minor version.
        /// The Patch version will be the fourth component of the version number. The
        /// build meta data will contain the third component of the version number if
        /// it is greater than zero.</returns>
        [Obsolete("This constructor is obsolete. Call FromVersion instead.")]
        public SemVersion(Version version)
        {
            if (version == null)
                throw new ArgumentNullException(nameof(version));

            Major = version.Major;
            Minor = version.Minor;

            if (version.Revision >= 0)
                Patch = version.Revision;

            Prerelease = "";

            Metadata = version.Build > 0 ? version.Build.ToString(CultureInfo.InvariantCulture) : "";
        }

        private SemVersion(int major, int minor, int patch,
            IReadOnlyList<PrereleaseIdentifier> prereleaseIdentifiers,
            IReadOnlyList<string> metadataIdentifiers)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
            Prerelease = string.Join(".", prereleaseIdentifiers);
            PrereleaseIdentifiers = prereleaseIdentifiers;
            Metadata = string.Join(".", metadataIdentifiers);
            MetadataIdentifiers = metadataIdentifiers;
        }

        #region System.Version
        /// <summary>
        /// Converts a <see cref="Version"/> into the equivalent semantic version.
        /// </summary>
        /// <param name="version">The version to be converted to a semantic version.</param>
        /// <returns>The equivalent semantic version.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="version"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="version"/> has a Revision greater than zero.</exception>
        /// <remarks>
        /// <see cref="Version"/> numbers have the form <em>major</em>.<em>minor</em>[.<em>build</em>[.<em>revision</em>]]
        /// where square brackets ('[' and ']')  indicate optional components. The first three parts
        /// are converted to the Major, Minor, and Patch versions of a semantic version. If the build component
        /// is not defined (-1), the Patch number is assumed to be zero. <see cref="Version"/> numbers
        /// with a revision component greater than zero cannot be converted to semantic versions. An
        /// <see cref="ArgumentException"/> is thrown when this method is called with such a <see cref="Version"/>.
        /// </remarks>
        public static SemVersion FromVersion(Version version)
        {
            if (version == null) throw new ArgumentNullException(nameof(version));
            if (version.Revision > 0) throw new ArgumentException("Version with Revision number can't be converted to SemVer.", nameof(version));
            var patch = version.Build > 0 ? version.Build : 0;
            return new SemVersion(version.Major, version.Minor, patch);
        }

        /// <summary>
        /// Converts this semantic version to a <see cref="Version"/>.
        /// </summary>
        /// <returns>The equivalent <see cref="Version"/>.</returns>
        /// <remarks>
        /// A semantic version of the form <em>major</em>.<em>minor</em>.<em>patch</em>
        /// is converted to a <see cref="Version"/> of the form
        /// <em>major</em>.<em>minor</em>.<em>build</em> where the build number is the
        /// patch version of the semantic version. Prerelease versions and build metadata
        /// are not representable in a <see cref="Version"/>. This method throws
        /// an <see cref="InvalidOperationException"/> if the semantic version is a
        /// prerelease version or has build metadata.
        /// </remarks>
        /// <exception cref="InvalidOperationException">The semantic version is a prerelease version
        /// or has build metadata or has a negative major, minor, or patch version.</exception>
        public Version ToVersion()
        {
            if (Major < 0 || Minor < 0 || Patch < 0) throw new InvalidOperationException("Negative version numbers can't be converted to System.Version.");
            if (IsPrerelease) throw new InvalidOperationException("Prerelease version can't be converted to System.Version.");
            if (Metadata.Length != 0) throw new InvalidOperationException("Version with build metadata can't be converted to System.Version.");

            return new Version(Major, Minor, Patch);
        }
        #endregion

        /// <summary>
        /// Converts the string representation of a semantic version to its <see cref="SemVersion"/>
        /// equivalent. Parsing is not strict, minor and patch version numbers are optional.
        /// </summary>
        /// <param name="version">The version string.</param>
        /// <returns>The <see cref="SemVersion"/> object.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="version"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The <paramref name="version"/> has an invalid format.</exception>
        /// <exception cref="OverflowException">The Major, Minor, or Patch versions are larger than <code>int.MaxValue</code>.</exception>
        public static SemVersion Parse(string version)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            return Parse(version, false);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        public static SemVersion Parse(string version, SemVersionStyles style)
        {
            if (!style.IsValid()) throw new ArgumentException(InvalidSemVersionStylesMessage, nameof(style));

            var ex = ParseVersion(version, style, null, out var semver);

            return ex is null ? semver : throw ex;
        }

        /// <summary>
        /// Converts the string representation of a semantic version to its <see cref="SemVersion"/> equivalent.
        /// </summary>
        /// <param name="version">The version string.</param>
        /// <param name="strict">If set to <see langword="true"/> minor and patch version are required,
        /// otherwise they are optional.</param>
        /// <returns>The <see cref="SemVersion"/> object.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="version"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The <paramref name="version"/> has an invalid format.</exception>
        /// <exception cref="InvalidOperationException">The <paramref name="version"/> is missing Minor or Patch versions and <paramref name="strict"/> is <see langword="true"/>.</exception>
        /// <exception cref="OverflowException">The Major, Minor, or Patch versions are larger than <code>int.MaxValue</code>.</exception>
        [Obsolete("Method is obsolete. Call Parse with SemVersionStyles instead.")]
        public static SemVersion Parse(string version, bool strict)
        {
            var match = ParseEx.Match(version);
            if (!match.Success)
                throw new ArgumentException("Invalid version.", nameof(version));

            var major = int.Parse(match.Groups["major"].Value, CultureInfo.InvariantCulture);

            var minorMatch = match.Groups["minor"];
            int minor = 0;
            if (minorMatch.Success)
                minor = int.Parse(minorMatch.Value, CultureInfo.InvariantCulture);
            else if (strict)
                throw new InvalidOperationException("Invalid version (no minor version given in strict mode)");

            var patchMatch = match.Groups["patch"];
            int patch = 0;
            if (patchMatch.Success)
                patch = int.Parse(patchMatch.Value, CultureInfo.InvariantCulture);
            else if (strict)
                throw new InvalidOperationException("Invalid version (no patch version given in strict mode)");

            var prerelease = match.Groups["pre"].Value;
            var metadata = match.Groups["metadata"].Value;

            return new SemVersion(major, minor, patch, prerelease, metadata);
        }

        /// <summary>
        /// Converts the string representation of a semantic version to its <see cref="SemVersion"/>
        /// equivalent and returns a value that indicates whether the conversion succeeded. Parsing
        /// is not strict, minor and patch version numbers are optional.
        /// </summary>
        /// <param name="version">The version string.</param>
        /// <param name="semver">When the method returns, contains a <see cref="SemVersion"/> instance equivalent
        /// to the version string passed in, if the version string was valid, or <see langword="null"/> if the
        /// version string was not valid.</param>
        /// <returns><see langword="false"/> when a invalid version string is passed, otherwise <see langword="true"/>.</returns>
        public static bool TryParse(string version, out SemVersion semver)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            return TryParse(version, out semver, false);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        public static bool TryParse(string version, SemVersionStyles style, out SemVersion semver)
        {
            if (!style.IsValid()) throw new ArgumentException(InvalidSemVersionStylesMessage, nameof(style));
            return ParseVersion(version, style, ParseFailedException, out semver) != null;
        }

        /// <summary>
        /// Converts the string representation of a semantic version to its <see cref="SemVersion"/>
        /// equivalent and returns a value that indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="version">The version string.</param>
        /// <param name="semver">When the method returns, contains a <see cref="SemVersion"/> instance equivalent
        /// to the version string passed in, if the version string was valid, or <see langword="null"/> if the
        /// version string was not valid.</param>
        /// <param name="strict">If set to <see langword="true"/> minor and patch version are required,
        /// otherwise they are optional.</param>
        /// <returns><see langword="false"/> when a invalid version string is passed, otherwise <see langword="true"/>.</returns>
        [Obsolete("Method is obsolete. Call TryParse with SemVersionStyle instead.")]
        public static bool TryParse(string version, out SemVersion semver, bool strict)
        {
            semver = null;
            if (version is null) return false;

            var match = ParseEx.Match(version);
            if (!match.Success) return false;

            if (!int.TryParse(match.Groups["major"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var major))
                return false;

            var minorMatch = match.Groups["minor"];
            int minor = 0;
            if (minorMatch.Success)
            {
                if (!int.TryParse(minorMatch.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out minor))
                    return false;
            }
            else if (strict) return false;

            var patchMatch = match.Groups["patch"];
            int patch = 0;
            if (patchMatch.Success)
            {
                if (!int.TryParse(patchMatch.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out patch))
                    return false;
            }
            else if (strict) return false;

            var prerelease = match.Groups["pre"].Value;
            var metadata = match.Groups["metadata"].Value;

            semver = new SemVersion(major, minor, patch, prerelease, metadata);
            return true;
        }

        #region Parsing Implementation
        /// <summary>
        /// The internal method that all parsing is based on. Because this is called by both
        /// <see cref="Parse(string, SemVersionStyles)"/> and <see cref="TryParse(string, SemVersionStyles, out SemVersion)"/>
        /// it does not throw exceptions, but instead returns the exception that should be thrown
        /// by the parse method. For performance when used from try parse, all exception construction
        /// and message formatting can be avoided by passing in an exception which will be returned
        /// when parsing fails.
        /// </summary>
        /// <remarks>This does not validate the <paramref name="style"/> parameter.
        /// That must be done in the calling method.</remarks>
        private static Exception ParseVersion(
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
            var parseEx = ParseNumber(version, ref i, allowLeadingZeros, ex, out var major);
            if (parseEx != null) return parseEx;

            // Parse minor version
            var minor = 0;
            if (i < version.Length && version[i] == '.')
            {
                i += 1;
                parseEx = ParseNumber(version, ref i, allowLeadingZeros, ex, out minor);
                if (parseEx != null) return parseEx;
            }
            else if (!style.HasStyle(SemVersionStyles.OptionalMinorPatch))
                return ex ?? NewFormatException(MinorVersionNotOptionalMessage, version);

            // Parse patch version
            var patch = 0;
            if (i < version.Length && version[i] == '.')
            {
                i += 1;
                parseEx = ParseNumber(version, ref i, allowLeadingZeros, ex, out patch);
                if (parseEx != null) return parseEx;
            }
            else if (!style.HasStyle(SemVersionStyles.OptionalPatch))
                return ex ?? NewFormatException(PatchVersionNotOptionalMessage, version);

            // Parse prerelease version
            var allowMultiplePrereleaseIdentifiers = !style.HasStyle(SemVersionStyles.DisallowMultiplePrereleaseIdentifiers);
            List<PrereleaseIdentifier> prereleaseIdentifiers;
            if (i < version.Length && version[i] == '-')
            {
                i += 1;
                prereleaseIdentifiers = ParsePrerelease(version, ref i, allowLeadingZeros, allowMultiplePrereleaseIdentifiers);
                if (prereleaseIdentifiers == null) return ex ?? new InvalidOperationException();
            }
            else
                prereleaseIdentifiers = new List<PrereleaseIdentifier>();

            // Parse metadata
            var allowMetadata = !style.HasStyle(SemVersionStyles.DisallowMetadata);
            List<string> metadataIdentifiers;
            if (allowMetadata && i < version.Length && version[i] == '+')
            {
                i += 1;
                metadataIdentifiers = ParseMetadata(version, ref i);
            }
            else
                metadataIdentifiers = new List<string>();

            // Check for extra characters at the end
            if (i != version.Length) return ex ?? new InvalidOperationException();

            semver = new SemVersion(major, minor, patch,
                new ReadOnlyCollection<PrereleaseIdentifier>(prereleaseIdentifiers),
                new ReadOnlyCollection<string>(metadataIdentifiers));
            return null;
        }

        private static Exception ParseNumber(
            string version,
            ref int i,
            bool allowLeadingZero,
            Exception ex,
            out int number)
        {
            var start = i;
            if (!allowLeadingZero && i < version.Length && version[i] == '0')
            {
                number = 0;
                return ex ?? NewFormatException(LeadingZeroMajorMinorPatchMessage, version);
            }

            while (i < version.Length && version[i].IsDigit())
                i += 1;

            if (start == i)
            {
                number = 0;
                return ex ?? NewFormatException(MissingMajorMinorPatchAfterDotMessage, version);
            }

            if (!int.TryParse(version.Substring(start, i - start), NumberStyles.None, CultureInfo.InvariantCulture, out number))
                // This parse should not fail, if it does, that is a bug
                throw new InvalidOperationException(NumberParseInvalid);

            return null;
        }

        private static List<PrereleaseIdentifier> ParsePrerelease(
            string version,
            ref int i,
            bool allowLeadingZero,
            bool allowMultiplePrereleaseIdentifiers)
        {
            var prereleaseIdentifiers = new List<PrereleaseIdentifier>();
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
                    else if (!c.IsDigit())
                        break;

                    i += 1;
                }

                // Empty identifiers not allowed
                if (s == i) return null;
                var identifier = version.Substring(s, i - s);
                if (!isNumeric)
                    prereleaseIdentifiers.Add(new PrereleaseIdentifier(identifier, null));
                else
                {
                    if (!allowLeadingZero && version[s] == '0') return null;
                    if (!int.TryParse(identifier, NumberStyles.None, null, out var intValue)) return null;
                    prereleaseIdentifiers.Add(new PrereleaseIdentifier(identifier.TrimStart('0'), intValue));
                }

            } while (i < version.Length && version[i] == '.' && allowMultiplePrereleaseIdentifiers);

            return prereleaseIdentifiers;
        }

        private static List<string> ParseMetadata(string version, ref int i)
        {
            var metadataIdentifiers = new List<string>();
            i -= 1; // Back up so we are before the start of the first identifier
            do
            {
                i += 1; // Advance to start of identifier
                var s = i;
                while (i < version.Length)
                {
                    var c = version[i];
                    if (!c.IsAlphaOrHyphen() && !c.IsDigit())
                        break;
                    i += 1;
                }

                // Empty identifiers not allowed
                if (s == i) return null;
                var identifier = version.Substring(s, i - s);
                metadataIdentifiers.Add(new PrereleaseIdentifier(identifier, null));

            } while (i < version.Length && version[i] == '.');

            return metadataIdentifiers;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static FormatException NewFormatException(string messageTemplate, string version)
        {
            return new FormatException(string.Format(CultureInfo.InvariantCulture, messageTemplate, version));
        }
        #endregion Parsing Implementation

        /// <summary>
        /// Checks whether two semantic versions are equal.
        /// </summary>
        /// <param name="versionA">The first version to compare.</param>
        /// <param name="versionB">The second version to compare.</param>
        /// <returns><see langword="true"/> if the two values are equal, otherwise <see langword="false"/>.</returns>
        public static bool Equals(SemVersion versionA, SemVersion versionB)
        {
            if (ReferenceEquals(versionA, versionB)) return true;
            if (versionA is null || versionB is null) return false;
            return versionA.Equals(versionB);
        }

        /// <summary>
        /// Compares the specified versions.
        /// </summary>
        /// <param name="versionA">The first version to compare.</param>
        /// <param name="versionB">The second version to compare.</param>
        /// <returns>A signed number indicating the relative values of <paramref name="versionA"/> and <paramref name="versionB"/>.</returns>
        public static int Compare(SemVersion versionA, SemVersion versionB)
        {
            if (ReferenceEquals(versionA, versionB)) return 0;
            if (versionA is null) return -1;
            if (versionB is null) return 1;
            return versionA.CompareTo(versionB);
        }

        /// <summary>
        /// Make a copy of the current instance with changed properties.
        /// </summary>
        /// <param name="major">The value to replace the major version or <see langword="null"/> to leave it unchanged.</param>
        /// <param name="minor">The value to replace the minor version or <see langword="null"/> to leave it unchanged.</param>
        /// <param name="patch">The value to replace the patch version or <see langword="null"/> to leave it unchanged.</param>
        /// <param name="prerelease">The value to replace the prerelease version or <see langword="null"/> to leave it unchanged.</param>
        /// <param name="build">The value to replace the build metadata or <see langword="null"/> to leave it unchanged.</param>
        /// <returns>The new version object.</returns>
        /// <remarks>
        /// The change method is intended to be called using named argument syntax, passing only
        /// those fields to be changed.
        /// </remarks>
        /// <example>
        /// To change only the patch version:
        /// <code>version.Change(patch: 4)</code>
        /// </example>
        public SemVersion Change(int? major = null, int? minor = null, int? patch = null,
            string prerelease = null, string build = null)
        {
            return new SemVersion(
                major ?? Major,
                minor ?? Minor,
                patch ?? Patch,
                prerelease ?? Prerelease,
                build ?? Metadata);
        }

        /// <summary>
        /// Gets the major version.
        /// </summary>
        /// <value>
        /// The major version.
        /// </value>
        public int Major { get; }

        /// <summary>
        /// Gets the minor version.
        /// </summary>
        /// <value>
        /// The minor version.
        /// </value>
        public int Minor { get; }

        /// <summary>
        /// Gets the patch version.
        /// </summary>
        /// <value>
        /// The patch version.
        /// </value>
        public int Patch { get; }

        /// <summary>
        /// Gets the prerelease label of this semantic version.
        /// </summary>
        /// <value>
        /// The prerelease label or empty string if this is a release version.
        /// </value>
        /// <remarks>
        /// A prerelease version label follows the main version number separated
        /// by a dash ('-'). It is a series of identifiers each of which may either
        /// be alphanumeric or numeric. Prerelease versions have lower precedence
        /// than release versions.
        /// </remarks>
        public string Prerelease { get; }

        public IReadOnlyList<PrereleaseIdentifier> PrereleaseIdentifiers { get; }

        /// <summary>
        /// Indicates whether this semantic version is a prerelease version.
        /// </summary>
        /// <value><see langword="true"/> if the <see cref="Prerelease"/> property
        /// is non-empty; <see langword="false"/> if it is empty.</value>
        public bool IsPrerelease => Prerelease.Length != 0;

        /// <summary>
        /// Gets the build metadata.
        /// </summary>
        /// <value>
        /// The build metadata or empty string if there is no build metadata.
        /// </value>
        [Obsolete("This property is obsolete. Use Metadata instead.")]
        public string Build => Metadata;

        /// <summary>
        /// Gets the build metadata of this semantic version.
        /// </summary>
        /// <value>The build metadata of this version or empty string if there
        /// is no metadata.</value>
        /// <remarks>
        /// The build metadata is a series of dot separated alphanumeric identifiers separated
        /// from the rest of the version number with a plus sign ('+').
        ///
        /// The metadata does not affect precedence. Two versions with different
        /// build metadata have the same precedence. However, metadata does affect
        /// sort order. A version without metadata sorts before one that has metadata.
        /// </remarks>
        public string Metadata { get; }

        public IReadOnlyList<string> MetadataIdentifiers { get; }

        /// <summary>
        /// Returns the <see cref="string" /> equivalent of this version.
        /// </summary>
        /// <returns>
        /// The <see cref="string" /> equivalent of this version.
        /// </returns>
        public override string ToString()
        {
            // Assume all separators ("..-+"), at most 2 extra chars
            var estimatedLength = 4 + Major.Digits() + Minor.Digits() + Patch.Digits()
                                  + Prerelease.Length + Metadata.Length;
            var version = new StringBuilder(estimatedLength);
            version.Append(Major);
            version.Append('.');
            version.Append(Minor);
            version.Append('.');
            version.Append(Patch);
            if (Prerelease.Length > 0)
            {
                version.Append('-');
                version.Append(Prerelease);
            }
            if (Metadata.Length > 0)
            {
                version.Append('+');
                version.Append(Metadata);
            }
            return version.ToString();
        }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates
        /// whether the current instance precedes, follows, or occurs in the same position in the sort order as the
        /// other object.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared.
        /// The return value has these meanings:
        ///  Less than zero: This instance precedes <paramref name="obj" /> in the sort order.
        ///  Zero: This instance occurs in the same position in the sort order as <paramref name="obj" />.
        ///  Greater than zero: This instance follows <paramref name="obj" /> in the sort order.
        /// </returns>
        /// <exception cref="InvalidCastException">The <paramref name="obj"/> is not a <see cref="SemVersion"/>.</exception>
        public int CompareTo(object obj)
        {
            return CompareTo((SemVersion)obj);
        }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates
        /// whether the current instance precedes, follows, or occurs in the same position in the sort order as the
        /// other object.
        /// </summary>
        /// <param name="other">An object to compare with this instance.</param>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared.
        /// The return value has these meanings:
        ///  Less than zero: This instance precedes <paramref name="other" /> in the sort order.
        ///  Zero: This instance occurs in the same position in the sort order as <paramref name="other" />.
        ///  Greater than zero: This instance follows <paramref name="other" /> in the sort order.
        /// </returns>
        public int CompareTo(SemVersion other)
        {
            var r = CompareByPrecedence(other);
            if (r != 0) return r;

#pragma warning disable CA1062 // Validate arguments of public methods
            // If other is null, CompareByPrecedence() returns 1
            return CompareComponent(Metadata, other.Metadata);
#pragma warning restore CA1062 // Validate arguments of public methods
        }

        /// <summary>
        /// Returns whether two semantic versions have the same precedence. Versions
        /// that differ only by build metadata have the same precedence.
        /// </summary>
        /// <param name="other">The semantic version to compare to.</param>
        /// <returns><see langword="true"/> if the version precedences are equal.</returns>
        public bool PrecedenceMatches(SemVersion other)
        {
            return CompareByPrecedence(other) == 0;
        }

        /// <summary>
        /// Compares two semantic versions by precedence as defined in the SemVer spec. Versions
        /// that differ only by build metadata have the same precedence.
        /// </summary>
        /// <param name="other">The semantic version.</param>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared.
        /// The return value has these meanings:
        ///  Less than zero: This instance precedes <paramref name="other" /> in the sort order.
        ///  Zero: This instance occurs in the same position in the sort order as <paramref name="other" />.
        ///  Greater than zero: This instance follows <paramref name="other" /> in the sort order.
        /// </returns>
        public int CompareByPrecedence(SemVersion other)
        {
            if (other is null)
                return 1;

            var r = Major.CompareTo(other.Major);
            if (r != 0) return r;

            r = Minor.CompareTo(other.Minor);
            if (r != 0) return r;

            r = Patch.CompareTo(other.Patch);
            if (r != 0) return r;

            return CompareComponent(Prerelease, other.Prerelease, true);
        }

        private static int CompareComponent(string a, string b, bool nonemptyIsLower = false)
        {
            var aEmpty = string.IsNullOrEmpty(a);
            var bEmpty = string.IsNullOrEmpty(b);
            if (aEmpty && bEmpty)
                return 0;

            if (aEmpty)
                return nonemptyIsLower ? 1 : -1;
            if (bEmpty)
                return nonemptyIsLower ? -1 : 1;

            var aComps = a.Split('.');
            var bComps = b.Split('.');

            var minLen = Math.Min(aComps.Length, bComps.Length);
            for (int i = 0; i < minLen; i++)
            {
                var ac = aComps[i];
                var bc = bComps[i];
                var aIsNum = int.TryParse(ac, out var aNum);
                var bIsNum = int.TryParse(bc, out var bNum);
                int r;
                if (aIsNum && bIsNum)
                {
                    r = aNum.CompareTo(bNum);
                    if (r != 0) return r;
                }
                else
                {
                    if (aIsNum)
                        return -1;
                    if (bIsNum)
                        return 1;
                    r = string.CompareOrdinal(ac, bc);
                    if (r != 0)
                        return r;
                }
            }

            return aComps.Length.CompareTo(bComps.Length);
        }

        /// <summary>
        /// Determines whether the specified <see cref="object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
        /// <returns>
        ///   <see langword="true"/> if the specified <see cref="object" /> is equal to this instance, otherwise <see langword="false"/>.
        /// </returns>
        /// <exception cref="InvalidCastException">The <paramref name="obj"/> is not a <see cref="SemVersion"/>.</exception>
        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            var other = (SemVersion)obj;

            return Major == other.Major
                && Minor == other.Minor
                && Patch == other.Patch
                && string.Equals(Prerelease, other.Prerelease, StringComparison.Ordinal)
                && string.Equals(Metadata, other.Metadata, StringComparison.Ordinal);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                // TODO verify this. Some versions start result = 17. Some use 37 instead of 31
                int result = Major.GetHashCode();
                result = result * 31 + Minor.GetHashCode();
                result = result * 31 + Patch.GetHashCode();
                result = result * 31 + Prerelease.GetHashCode();
                result = result * 31 + Metadata.GetHashCode();
                return result;
            }
        }

#if !NETSTANDARD
        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination (see <see cref="SerializationInfo"/>) for this serialization.</param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            info.AddValue("SemVersion", ToString());
        }
#endif

#pragma warning disable CA2225 // Operator overloads have named alternates
        /// <summary>
        /// Implicit conversion from <see cref="string"/> to <see cref="SemVersion"/>.
        /// </summary>
        /// <param name="version">The semantic version.</param>
        /// <returns>The <see cref="SemVersion"/> object.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="version"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The version number has an invalid format.</exception>
        /// <exception cref="OverflowException">The Major, Minor, or Patch versions are larger than <code>int.MaxValue</code>.</exception>
        [Obsolete("Implicit conversion from string is obsolete. Use Parse or TryParse method instead.")]
        public static implicit operator SemVersion(string version)
#pragma warning restore CA2225 // Operator overloads have named alternates
        {
            return Parse(version, false);
        }

        /// <summary>
        /// Compares two semantic versions for equality.
        /// </summary>
        /// <param name="left">The left value.</param>
        /// <param name="right">The right value.</param>
        /// <returns>If left is equal to right <see langword="true"/>, otherwise <see langword="false"/>.</returns>
        public static bool operator ==(SemVersion left, SemVersion right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Compares two semantic versions for inequality.
        /// </summary>
        /// <param name="left">The left value.</param>
        /// <param name="right">The right value.</param>
        /// <returns>If left is not equal to right <see langword="true"/>, otherwise <see langword="false"/>.</returns>
        public static bool operator !=(SemVersion left, SemVersion right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Compares two semantic versions.
        /// </summary>
        /// <param name="left">The left value.</param>
        /// <param name="right">The right value.</param>
        /// <returns>If left is greater than right <see langword="true"/>, otherwise <see langword="false"/>.</returns>
        public static bool operator >(SemVersion left, SemVersion right)
        {
            return Compare(left, right) > 0;
        }

        /// <summary>
        /// Compares two semantic versions.
        /// </summary>
        /// <param name="left">The left value.</param>
        /// <param name="right">The right value.</param>
        /// <returns>If left is greater than or equal to right <see langword="true"/>, otherwise <see langword="false"/>.</returns>
        public static bool operator >=(SemVersion left, SemVersion right)
        {
            return Equals(left, right) || Compare(left, right) > 0;
        }

        /// <summary>
        /// Compares two semantic versions.
        /// </summary>
        /// <param name="left">The left value.</param>
        /// <param name="right">The right value.</param>
        /// <returns>If left is less than right <see langword="true"/>, otherwise <see langword="false"/>.</returns>
        public static bool operator <(SemVersion left, SemVersion right)
        {
            return Compare(left, right) < 0;
        }

        /// <summary>
        /// Compares two semantic versions.
        /// </summary>
        /// <param name="left">The left value.</param>
        /// <param name="right">The right value.</param>
        /// <returns>If left is less than or equal to right <see langword="true"/>, otherwise <see langword="false"/>.</returns>
        public static bool operator <=(SemVersion left, SemVersion right)
        {
            return Equals(left, right) || Compare(left, right) < 0;
        }
    }
}
