using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
#if SERIALIZABLE
using System.Runtime.Serialization;
using System.Security.Permissions;
#endif
using System.Text.RegularExpressions;
using Semver.Comparers;
using Semver.Ranges;
using Semver.Ranges.Npm;
using Semver.Utility;

namespace Semver
{
    /// <summary>
    /// A semantic version number. Conforms with v2.0.0 of semantic versioning
    /// (<a href="https://semver.org">semver.org</a>).
    /// </summary>
#if SERIALIZABLE
    [Serializable]
    public sealed class SemVersion : IComparable<SemVersion>, IComparable, IEquatable<SemVersion>, ISerializable
#else
    public sealed class SemVersion : IComparable<SemVersion>, IComparable, IEquatable<SemVersion>
#endif
    {
        internal static readonly SemVersion Min = new SemVersion(0, 0, 0, new[] { new PrereleaseIdentifier(0) });
        internal static readonly SemVersion MinRelease = new SemVersion(0, 0, 0);
        internal static readonly SemVersion Max = new SemVersion(int.MaxValue, int.MaxValue, int.MaxValue);

        internal const string InvalidSemVersionStylesMessage = "An invalid SemVersionStyles value was used.";
        private const string InvalidMajorVersionMessage = "Major version must be greater than or equal to zero.";
        private const string InvalidMinorVersionMessage = "Minor version must be greater than or equal to zero.";
        private const string InvalidPatchVersionMessage = "Patch version must be greater than or equal to zero.";
        private const string PrereleaseIdentifierIsDefaultMessage = "Prerelease identifier cannot be default/null.";
        private const string MetadataIdentifierIsDefaultMessage = "Metadata identifier cannot be default/null.";
        // TODO include in v3.0.0 for issue #72
        //internal const string InvalidMaxLengthMessage = "Must not be negative.";
        internal const int MaxVersionLength = 1024;

        private static readonly Regex ParseRegex =
            new Regex(@"^(?<major>\d+)" +
                @"(?>\.(?<minor>\d+))?" +
                @"(?>\.(?<patch>\d+))?" +
                @"(?>\-(?<pre>[0-9A-Za-z\-\.]+))?" +
                @"(?>\+(?<metadata>[0-9A-Za-z\-\.]+))?$",
#if COMPILED_REGEX
                RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.ExplicitCapture,
#else
                RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture,
#endif
                TimeSpan.FromSeconds(0.5));

#if SERIALIZABLE
        /// <summary>
        /// Deserialize a <see cref="SemVersion"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">The <paramref name="info"/> parameter is null.</exception>
        private SemVersion(SerializationInfo info, StreamingContext context)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
#pragma warning disable CS0618 // Type or member is obsolete
            var semVersion = Parse(info.GetString("SemVersion"), true);
#pragma warning restore CS0618 // Type or member is obsolete
            Major = semVersion.Major;
            Minor = semVersion.Minor;
            Patch = semVersion.Patch;
            Prerelease = semVersion.Prerelease;
            PrereleaseIdentifiers = semVersion.PrereleaseIdentifiers;
            Metadata = semVersion.Metadata;
            MetadataIdentifiers = semVersion.MetadataIdentifiers;
        }

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

        /// <summary>
        /// Constructs a new instance of the <see cref="SemVersion" /> class.
        /// </summary>
        /// <param name="major">The major version number.</param>
        // Constructor needed to resolve ambiguity between other overloads with default parameters.
        public SemVersion(int major)
        {
            Major = major;
            Minor = 0;
            Patch = 0;
            Prerelease = "";
            PrereleaseIdentifiers = ReadOnlyList<PrereleaseIdentifier>.Empty;
            Metadata = "";
            MetadataIdentifiers = ReadOnlyList<MetadataIdentifier>.Empty;
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="SemVersion" /> class.
        /// </summary>
        /// <param name="major">The major version number.</param>
        /// <param name="minor">The minor version number.</param>
        // Constructor needed to resolve ambiguity between other overloads with default parameters.
        public SemVersion(int major, int minor)
        {
            Major = major;
            Minor = minor;
            Patch = 0;
            Prerelease = "";
            PrereleaseIdentifiers = ReadOnlyList<PrereleaseIdentifier>.Empty;
            Metadata = "";
            MetadataIdentifiers = ReadOnlyList<MetadataIdentifier>.Empty;
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="SemVersion" /> class.
        /// </summary>
        /// <param name="major">The major version number.</param>
        /// <param name="minor">The minor version number.</param>
        /// <param name="patch">The patch version number.</param>
        // Constructor needed to resolve ambiguity between other overloads with default parameters.
        public SemVersion(int major, int minor, int patch)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
            Prerelease = "";
            PrereleaseIdentifiers = ReadOnlyList<PrereleaseIdentifier>.Empty;
            Metadata = "";
            MetadataIdentifiers = ReadOnlyList<MetadataIdentifier>.Empty;
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="SemVersion" /> class.
        /// </summary>
        /// <param name="major">The major version number.</param>
        /// <param name="minor">The minor version number.</param>
        /// <param name="patch">The patch version number.</param>
        /// <param name="prerelease">The prerelease portion (e.g. "alpha.5").</param>
        /// <param name="build">The build metadata (e.g. "nightly.232").</param>
        [EditorBrowsable(EditorBrowsableState.Never), Obsolete("This constructor is obsolete. Use another constructor or SemVersion.ParsedFrom() instead.")]
        public SemVersion(int major, int minor = 0, int patch = 0, string prerelease = "", string build = "")
        {
            Major = major;
            Minor = minor;
            Patch = patch;

            prerelease = prerelease ?? "";
            Prerelease = prerelease;
            PrereleaseIdentifiers = prerelease.SplitAndMapToReadOnlyList('.', PrereleaseIdentifier.CreateLoose);

            build = build ?? "";
            Metadata = build;
            MetadataIdentifiers = build.SplitAndMapToReadOnlyList('.', MetadataIdentifier.CreateLoose);
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="SemVersion" /> class.
        /// </summary>
        /// <param name="major">The major version number.</param>
        /// <param name="minor">The minor version number.</param>
        /// <param name="patch">The patch version number.</param>
        /// <param name="prerelease">The prerelease identifiers.</param>
        /// <param name="metadata">The build metadata identifiers.</param>
        /// <exception cref="ArgumentOutOfRangeException">A <paramref name="major"/>,
        /// <paramref name="minor"/>, or <paramref name="patch"/> version number is negative.</exception>
        /// <exception cref="ArgumentException">A prerelease or metadata identifier has the default value.</exception>
        public SemVersion(int major, int minor = 0, int patch = 0,
            IEnumerable<PrereleaseIdentifier> prerelease = null,
            IEnumerable<MetadataIdentifier> metadata = null)
        {
            if (major < 0) throw new ArgumentOutOfRangeException(nameof(major), InvalidMajorVersionMessage);
            if (minor < 0) throw new ArgumentOutOfRangeException(nameof(minor), InvalidMinorVersionMessage);
            if (patch < 0) throw new ArgumentOutOfRangeException(nameof(patch), InvalidPatchVersionMessage);
            IReadOnlyList<PrereleaseIdentifier> prereleaseIdentifiers;
            if (prerelease is null)
                prereleaseIdentifiers = null;
            else
            {
                prereleaseIdentifiers = prerelease.ToReadOnlyList();
                if (prereleaseIdentifiers.Any(i => i == default))
                    throw new ArgumentException(PrereleaseIdentifierIsDefaultMessage, nameof(prerelease));
            }

            IReadOnlyList<MetadataIdentifier> metadataIdentifiers;
            if (metadata is null)
                metadataIdentifiers = null;
            else
            {
                metadataIdentifiers = metadata.ToReadOnlyList();
                if (metadataIdentifiers.Any(i => i == default))
                    throw new ArgumentException(MetadataIdentifierIsDefaultMessage, nameof(metadata));
            }

            Major = major;
            Minor = minor;
            Patch = patch;

            if (prereleaseIdentifiers is null || prereleaseIdentifiers.Count == 0)
            {
                Prerelease = "";
                PrereleaseIdentifiers = ReadOnlyList<PrereleaseIdentifier>.Empty;
            }
            else
            {
                Prerelease = string.Join(".", prereleaseIdentifiers);
                PrereleaseIdentifiers = prereleaseIdentifiers;
            }

            if (metadataIdentifiers is null || metadataIdentifiers.Count == 0)
            {
                Metadata = "";
                MetadataIdentifiers = ReadOnlyList<MetadataIdentifier>.Empty;
            }
            else
            {
                Metadata = string.Join(".", metadataIdentifiers);
                MetadataIdentifiers = metadataIdentifiers;
            }
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="SemVersion" /> class.
        /// </summary>
        /// <param name="major">The major version number.</param>
        /// <param name="minor">The minor version number.</param>
        /// <param name="patch">The patch version number.</param>
        /// <param name="prerelease">The prerelease identifiers.</param>
        /// <param name="metadata">The build metadata identifiers.</param>
        /// <exception cref="ArgumentOutOfRangeException">A <paramref name="major"/>,
        /// <paramref name="minor"/>, or <paramref name="patch"/> version number is negative.</exception>
        /// <exception cref="ArgumentNullException">One of the prerelease or metadata identifiers is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">A prerelease identifier is empty or contains invalid
        /// characters (i.e. characters that are not ASCII alphanumerics or hyphens) or has leading
        /// zeros for a numeric identifier. Or, a metadata identifier is empty or contains invalid
        /// characters (i.e. characters that are not ASCII alphanumerics or hyphens).</exception>
        /// <exception cref="OverflowException">A numeric prerelease identifier value is too large
        /// for <see cref="Int32"/>.</exception>
        public SemVersion(int major, int minor = 0, int patch = 0,
            IEnumerable<string> prerelease = null,
            IEnumerable<string> metadata = null)
        {
            if (major < 0) throw new ArgumentOutOfRangeException(nameof(major), InvalidMajorVersionMessage);
            if (minor < 0) throw new ArgumentOutOfRangeException(nameof(minor), InvalidMinorVersionMessage);
            if (patch < 0) throw new ArgumentOutOfRangeException(nameof(patch), InvalidPatchVersionMessage);
            var prereleaseIdentifiers = prerelease?
                                        .Select(i => new PrereleaseIdentifier(i, allowLeadingZeros: false, nameof(prerelease)))
                                        .ToReadOnlyList();

            var metadataIdentifiers = metadata?
                                      .Select(i => new MetadataIdentifier(i, nameof(metadata)))
                                      .ToReadOnlyList();

            Major = major;
            Minor = minor;
            Patch = patch;

            if (prereleaseIdentifiers is null || prereleaseIdentifiers.Count == 0)
            {
                Prerelease = "";
                PrereleaseIdentifiers = ReadOnlyList<PrereleaseIdentifier>.Empty;
            }
            else
            {
                Prerelease = string.Join(".", prereleaseIdentifiers);
                PrereleaseIdentifiers = prereleaseIdentifiers;
            }

            if (metadataIdentifiers is null || metadataIdentifiers.Count == 0)
            {
                Metadata = "";
                MetadataIdentifiers = ReadOnlyList<MetadataIdentifier>.Empty;
            }
            else
            {
                Metadata = string.Join(".", metadataIdentifiers);
                MetadataIdentifiers = metadataIdentifiers;
            }
        }

        /// <summary>
        /// Create a new instance of the <see cref="SemVersion" /> class. Parses prerelease
        /// and metadata identifiers from dot separated strings. If parsing is not needed, use a
        /// constructor instead.
        /// </summary>
        /// <param name="major">The major version number.</param>
        /// <param name="minor">The minor version number.</param>
        /// <param name="patch">The patch version number.</param>
        /// <param name="prerelease">The prerelease portion (e.g. "alpha.5").</param>
        /// <param name="metadata">The build metadata (e.g. "nightly.232").</param>
        /// <param name="allowLeadingZeros">Allow leading zeros in numeric prerelease identifiers. Leading
        /// zeros will be removed.</param>
        /// <exception cref="ArgumentOutOfRangeException">A <paramref name="major"/>,
        /// <paramref name="minor"/>, or <paramref name="patch"/> version number is negative.</exception>
        /// <exception cref="ArgumentException">A prerelease identifier is empty or contains invalid
        /// characters (i.e. characters that are not ASCII alphanumerics or hyphens) or has leading
        /// zeros for a numeric identifier when <paramref name="allowLeadingZeros"/> is
        /// <see langword="false"/>. Or, a metadata identifier is empty or contains invalid
        /// characters (i.e. characters that are not ASCII alphanumerics or hyphens).</exception>
        /// <exception cref="OverflowException">A numeric prerelease identifier value is too large
        /// for <see cref="int"/>.</exception>
        public static SemVersion ParsedFrom(int major, int minor = 0, int patch = 0,
            string prerelease = "", string metadata = "", bool allowLeadingZeros = false)
        {
            if (major < 0) throw new ArgumentOutOfRangeException(nameof(major), InvalidMajorVersionMessage);
            if (minor < 0) throw new ArgumentOutOfRangeException(nameof(minor), InvalidMinorVersionMessage);
            if (patch < 0) throw new ArgumentOutOfRangeException(nameof(patch), InvalidPatchVersionMessage);

            if (prerelease is null) throw new ArgumentNullException(nameof(prerelease));
            var prereleaseIdentifiers = prerelease.Length == 0
                ? ReadOnlyList<PrereleaseIdentifier>.Empty
                : prerelease.SplitAndMapToReadOnlyList('.',
                    i => new PrereleaseIdentifier(i, allowLeadingZeros, nameof(prerelease)));
            if (allowLeadingZeros)
                // Leading zeros may have been removed, need to reconstruct the prerelease string
                prerelease = string.Join(".", prereleaseIdentifiers);

            if (metadata is null) throw new ArgumentNullException(nameof(metadata));
            var metadataIdentifiers = metadata.Length == 0
                ? ReadOnlyList<MetadataIdentifier>.Empty
                : metadata.SplitAndMapToReadOnlyList('.', i => new MetadataIdentifier(i, nameof(metadata)));

            return new SemVersion(major, minor, patch,
                prerelease, prereleaseIdentifiers, metadata, metadataIdentifiers);
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="SemVersion"/> class from
        /// a <see cref="Version"/>.
        /// </summary>
        /// <param name="version"><see cref="Version"/> used to initialize
        /// the major, minor, and patch version numbers and the build metadata.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="version"/> is null.</exception>
        /// <remarks>Constructs a <see cref="SemVersion"/> with the same major and
        /// minor version numbers. The patch version number will be the fourth component
        /// of the <paramref name="version"/>. The build meta data will contain the third component
        /// of the <paramref name="version"/> if it is greater than zero.</remarks>
        [EditorBrowsable(EditorBrowsableState.Never), Obsolete("This constructor is obsolete. Use SemVersion.FromVersion() instead.")]
        public SemVersion(Version version)
        {
            if (version == null)
                throw new ArgumentNullException(nameof(version));

            Major = version.Major;
            Minor = version.Minor;

            if (version.Revision >= 0)
                Patch = version.Revision;

            Prerelease = "";
            PrereleaseIdentifiers = ReadOnlyList<PrereleaseIdentifier>.Empty;

            if (version.Build > 0)
            {
                Metadata = version.Build.ToString(CultureInfo.InvariantCulture);
                MetadataIdentifiers = new List<MetadataIdentifier>(1) { MetadataIdentifier.CreateUnsafe(Metadata) }.AsReadOnly();
            }
            else
            {
                Metadata = "";
                MetadataIdentifiers = ReadOnlyList<MetadataIdentifier>.Empty;
            }
        }

        /// <summary>
        /// Construct a <see cref="SemVersion"/> from its proper parts.
        /// </summary>
        /// <remarks>Parameter validation is not performed. The <paramref name="major"/>,
        /// <paramref name="minor"/>, and <paramref name="patch"/> version numbers must not be
        /// negative. The <paramref name="prereleaseIdentifiers"/> and
        /// <paramref name="metadataIdentifiers"/> must not be <see langword="null"/> or
        /// contain invalid values and must be immutable. The <paramref name="prerelease"/>
        /// and <paramref name="metadata"/> must not be null and must be equal to the
        /// corresponding identifiers.</remarks>
        internal SemVersion(int major, int minor, int patch,
            string prerelease, IReadOnlyList<PrereleaseIdentifier> prereleaseIdentifiers,
            string metadata, IReadOnlyList<MetadataIdentifier> metadataIdentifiers)
        {
#if DEBUG
            if (major < 0) throw new ArgumentException("DEBUG: " + InvalidMajorVersionMessage, nameof(major));
            if (minor < 0) throw new ArgumentException("DEBUG: " + InvalidMinorVersionMessage, nameof(minor));
            if (patch < 0) throw new ArgumentException("DEBUG: " + InvalidPatchVersionMessage, nameof(patch));
            if (prerelease is null) throw new ArgumentNullException(nameof(prerelease), "DEBUG: Value cannot be null.");
            if (prereleaseIdentifiers is null) throw new ArgumentNullException(nameof(prereleaseIdentifiers), "DEBUG: Value cannot be null.");
            if (prereleaseIdentifiers.Any(i => i==default)) throw new ArgumentException("DEBUG: " + PrereleaseIdentifierIsDefaultMessage, nameof(prereleaseIdentifiers));
            if (prerelease != string.Join(".", prereleaseIdentifiers)) throw new ArgumentException($"DEBUG: must be equal to {nameof(prereleaseIdentifiers)}", nameof(prerelease));
            if (metadata is null) throw new ArgumentNullException(nameof(metadata), "DEBUG: Value cannot be null.");
            if (metadataIdentifiers is null) throw new ArgumentNullException(nameof(metadataIdentifiers), "DEBUG: Value cannot be null.");
            if (metadataIdentifiers.Any(i => i == default)) throw new ArgumentException("DEBUG: " + MetadataIdentifierIsDefaultMessage, nameof(metadataIdentifiers));
            if (metadata != string.Join(".", metadataIdentifiers)) throw new ArgumentException($"DEBUG: must be equal to {nameof(metadataIdentifiers)}", nameof(metadata));
#endif
            Major = major;
            Minor = minor;
            Patch = patch;
            Prerelease = prerelease;
            PrereleaseIdentifiers = prereleaseIdentifiers;
            Metadata = metadata;
            MetadataIdentifiers = metadataIdentifiers;
        }

        #region System.Version
        /// <summary>
        /// Converts a <see cref="Version"/> into the equivalent semantic version.
        /// </summary>
        /// <param name="version">The version to be converted to a semantic version.</param>
        /// <returns>The equivalent semantic version.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="version"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="version"/> has a revision number greater than zero.</exception>
        /// <remarks>
        /// <see cref="Version"/> numbers have the form <em>major</em>.<em>minor</em>[.<em>build</em>[.<em>revision</em>]]
        /// where square brackets ('[' and ']')  indicate optional components. The first three parts
        /// are converted to the major, minor, and patch version numbers of a semantic version. If the
        /// build component is not defined (-1), the patch number is assumed to be zero.
        /// <see cref="Version"/> numbers with a revision greater than zero cannot be converted to
        /// semantic versions. An <see cref="ArgumentException"/> is thrown when this method is called
        /// with such a <see cref="Version"/>.
        /// </remarks>
        public static SemVersion FromVersion(Version version)
        {
            if (version is null) throw new ArgumentNullException(nameof(version));
            if (version.Revision > 0) throw new ArgumentException("Version with Revision number can't be converted to SemVer.", nameof(version));
            var patch = version.Build > 0 ? version.Build : 0;
            return new SemVersion(version.Major, version.Minor, patch);
        }

        /// <summary>
        /// Converts this semantic version to a <see cref="Version"/>.
        /// </summary>
        /// <returns>The equivalent <see cref="Version"/>.</returns>
        /// <exception cref="InvalidOperationException">The semantic version is a prerelease version
        /// or has build metadata or has a negative major, minor, or patch version number.</exception>
        /// <remarks>
        /// A semantic version of the form <em>major</em>.<em>minor</em>.<em>patch</em>
        /// is converted to a <see cref="Version"/> of the form
        /// <em>major</em>.<em>minor</em>.<em>build</em> where the build number is the
        /// patch version of the semantic version. Prerelease versions and build metadata
        /// are not representable in a <see cref="Version"/>. This method throws
        /// an <see cref="InvalidOperationException"/> if the semantic version is a
        /// prerelease version or has build metadata.
        /// </remarks>
        public Version ToVersion()
        {
            if (Major < 0 || Minor < 0 || Patch < 0) throw new InvalidOperationException("Negative version numbers can't be converted to System.Version.");
            if (IsPrerelease) throw new InvalidOperationException("Prerelease version can't be converted to System.Version.");
            if (Metadata.Length != 0) throw new InvalidOperationException("Version with build metadata can't be converted to System.Version.");

            return new Version(Major, Minor, Patch);
        }
        #endregion

        /// <summary>
        /// Converts the string representation of a semantic version to its <see cref="SemVersion"/> equivalent.
        /// </summary>
        /// <param name="version">The version string.</param>
        /// <param name="style">A bitwise combination of enumeration values that indicates the style
        /// elements that can be present in <paramref name="version"/>. The preferred value to use
        /// is <see cref="SemVersionStyles.Strict"/>.</param>
        /// <param name="maxLength">The maximum length of <paramref name="version"/> that should be
        /// parsed. This prevents attacks using very long version strings.</param>
        /// <exception cref="ArgumentException"><paramref name="style"/> is not a valid
        /// <see cref="SemVersionStyles"/> value.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="version"/> is <see langword="null"/>.</exception>
        /// <exception cref="FormatException">The <paramref name="version"/> is invalid or not in a
        /// format compliant with <paramref name="style"/>.</exception>
        /// <exception cref="OverflowException">A numeric part of <paramref name="version"/> is too
        /// large for an <see cref="Int32"/>.</exception>
        public static SemVersion Parse(string version, SemVersionStyles style, int maxLength = MaxVersionLength)
        {
            if (!style.IsValid()) throw new ArgumentException(InvalidSemVersionStylesMessage, nameof(style));
            // TODO include in v3.0.0 for issue #72
            //if (maxLength < 0) throw new ArgumentOutOfRangeException(InvalidMaxLengthMessage, nameof(maxLength));
            var ex = SemVersionParser.Parse(version, style, null, maxLength, out var semver);

            return ex is null ? semver : throw ex;
        }

        /// <summary>
        /// Converts the string representation of a semantic version to its <see cref="SemVersion"/> equivalent.
        /// </summary>
        /// <param name="version">The version string.</param>
        /// <param name="strict">If set to <see langword="true"/>, minor and patch version are required;
        /// otherwise they are optional.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="version"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The <paramref name="version"/> has an invalid format.</exception>
        /// <exception cref="InvalidOperationException">The <paramref name="version"/> is missing minor
        /// or patch version numbers when <paramref name="strict"/> is <see langword="true"/>.</exception>
        /// <exception cref="OverflowException">The major, minor, or patch version number is larger
        /// than <see cref="int.MaxValue"/>.</exception>
        [EditorBrowsable(EditorBrowsableState.Never), Obsolete("Method is obsolete. Use Parse() overload with SemVersionStyles instead.")]
        public static SemVersion Parse(string version, bool strict = false)
        {
            var match = ParseRegex.Match(version);
            if (!match.Success)
                throw new ArgumentException($"Invalid version '{version}'.", nameof(version));

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
        /// equivalent. The return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="version">The version string.</param>
        /// <param name="style">A bitwise combination of enumeration values that indicates the style
        /// elements that can be present in <paramref name="version"/>. The preferred value to use
        /// is <see cref="SemVersionStyles.Strict"/>.</param>4
        /// <param name="semver">When this method returns, contains a <see cref="SemVersion"/> instance equivalent
        /// to the version string passed in, if the version string was valid, or <see langword="null"/> if the
        /// version string was invalid.</param>
        /// <param name="maxLength">The maximum length of <paramref name="version"/> that should be
        /// parsed. This prevents attacks using very long version strings.</param>
        public static bool TryParse(string version, SemVersionStyles style,
            out SemVersion semver, int maxLength = MaxVersionLength)
        {
            if (!style.IsValid()) throw new ArgumentException(InvalidSemVersionStylesMessage, nameof(style));
            // TODO include in v3.0.0 for issue #72
            //if (maxLength < 0) throw new ArgumentOutOfRangeException(InvalidMaxLengthMessage, nameof(maxLength));
            var exception = SemVersionParser.Parse(version, style, Parsing.FailedException, maxLength, out semver);

#if DEBUG
            // This check ensures that SemVersionParser.Parse doesn't construct an exception, but always returns ParseFailedException
            if (exception != null && exception != Parsing.FailedException)
                throw new InvalidOperationException($"DEBUG: {nameof(SemVersionParser)}.{nameof(SemVersionParser.Parse)} returned exception other than {nameof(Parsing.FailedException)}", exception);
#endif

            return exception is null;
        }

        /// <summary>
        /// Converts the string representation of a semantic version to its <see cref="SemVersion"/>
        /// equivalent. The return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="version">The version string.</param>
        /// <param name="semver">When this method returns, contains a <see cref="SemVersion"/> instance equivalent
        /// to the version string passed in, if the version string was valid, or <see langword="null"/> if the
        /// version string was invalid.</param>
        /// <param name="strict">If set to <see langword="true"/>, minor and patch version numbers are required;
        /// otherwise they are optional.</param>
        /// <returns><see langword="false"/> when a invalid version string is passed, otherwise <see langword="true"/>.</returns>
        [EditorBrowsable(EditorBrowsableState.Never), Obsolete("Method is obsolete. Use TryParse() overload with SemVersionStyles instead.")]
        public static bool TryParse(string version, out SemVersion semver, bool strict = false)
        {
            semver = null;
            if (version is null) return false;

            var match = ParseRegex.Match(version);
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

        /// <summary>
        /// Compares two versions and indicates whether the first precedes, follows, or is
        /// equal to the other in the sort order. Note that sort order is more specific than precedence order.
        /// </summary>
        /// <returns>
        /// An integer that indicates whether <paramref name="versionA"/> precedes, follows, or
        /// is equal to <paramref name="versionB"/> in the sort order.
        /// <list type="table">
        ///     <listheader>
        ///         <term>Value</term>
        ///         <description>Condition</description>
        ///     </listheader>
        ///     <item>
        ///         <term>Less than zero</term>
        ///         <description><paramref name="versionA"/> precedes <paramref name="versionB"/> in the sort order.</description>
        ///     </item>
        ///     <item>
        ///         <term>Zero</term>
        ///         <description><paramref name="versionA"/> is equal to <paramref name="versionB"/>.</description>
        ///     </item>
        ///     <item>
        ///         <term>Greater than zero</term>
        ///         <description>
        ///             <paramref name="versionA"/> follows <paramref name="versionB"/> in the sort order
        ///             or <paramref name="versionB"/> is <see langword="null" />.
        ///         </description>
        ///     </item>
        /// </list>
        /// </returns>
        /// <include file='SemVersionDocParts.xml' path='docParts/part[@id="SortOrder"]/*'/>
        [EditorBrowsable(EditorBrowsableState.Never), Obsolete("Method is obsolete. Use CompareSortOrder() or ComparePrecedence() instead.")]
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
        /// <param name="major">The value to replace the major version number or
        /// <see langword="null"/> to leave it unchanged.</param>
        /// <param name="minor">The value to replace the minor version number or
        /// <see langword="null"/> to leave it unchanged.</param>
        /// <param name="patch">The value to replace the patch version number or
        /// <see langword="null"/> to leave it unchanged.</param>
        /// <param name="prerelease">The value to replace the prerelease portion
        /// or <see langword="null"/> to leave it unchanged.</param>
        /// <param name="build">The value to replace the build metadata or <see langword="null"/>
        /// to leave it unchanged.</param>
        /// <returns>The new version with changed properties.</returns>
        /// <remarks>
        /// The change method is intended to be called using named argument syntax, passing only
        /// those fields to be changed.
        /// </remarks>
        /// <example>
        /// To change only the patch version:
        /// <code>var changedVersion = version.Change(patch: 4);</code>
        /// </example>
        [EditorBrowsable(EditorBrowsableState.Never), Obsolete("Method is obsolete. Use With() or With...() method instead.")]
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
        /// Creates a copy of the current instance with multiple changed properties. If changing only
        /// one property use one of the more specific <c>WithX()</c> methods.
        /// </summary>
        /// <param name="major">The value to replace the major version number or <see langword="null"/> to leave it unchanged.</param>
        /// <param name="minor">The value to replace the minor version number or <see langword="null"/> to leave it unchanged.</param>
        /// <param name="patch">The value to replace the patch version number or <see langword="null"/> to leave it unchanged.</param>
        /// <param name="prerelease">The value to replace the prerelease identifiers or <see langword="null"/> to leave it unchanged.</param>
        /// <param name="metadata">The value to replace the build metadata identifiers or <see langword="null"/> to leave it unchanged.</param>
        /// <returns>The new version with changed properties.</returns>
        /// <exception cref="ArgumentOutOfRangeException">A <paramref name="major"/>,
        /// <paramref name="minor"/>, or <paramref name="patch"/> version number is negative.</exception>
        /// <exception cref="ArgumentException">A prerelease or metadata identifier has the default value.</exception>
        /// <exception cref="OverflowException">A numeric prerelease identifier value is too large
        /// for <see cref="int"/>.</exception>
        /// <remarks>
        /// The <see cref="With"/> method is intended to be called using named argument syntax, passing only
        /// those fields to be changed.
        /// </remarks>
        /// <example>
        /// To change the minor and patch versions:
        /// <code>var modifiedVersion = version.With(minor: 2, patch: 4);</code>
        /// </example>
        public SemVersion With(
            int? major = null,
            int? minor = null,
            int? patch = null,
            IEnumerable<PrereleaseIdentifier> prerelease = null,
            IEnumerable<MetadataIdentifier> metadata = null)
        {
            // Note: It is tempting to null coalesce first, but then this method would report invalid
            // arguments on invalid SemVersion instances.
            if (major is int majorInt && majorInt < 0)
                throw new ArgumentOutOfRangeException(nameof(major), InvalidMajorVersionMessage);
            if (minor is int minorInt && minorInt < 0)
                throw new ArgumentOutOfRangeException(nameof(minor), InvalidMinorVersionMessage);
            if (patch is int patchInt && patchInt < 0)
                throw new ArgumentOutOfRangeException(nameof(patch), InvalidPatchVersionMessage);

            IReadOnlyList<PrereleaseIdentifier> prereleaseIdentifiers = null;
            string prereleaseString = null;
            if (prerelease != null)
            {
                prereleaseIdentifiers = prerelease.ToReadOnlyList();
                if (prereleaseIdentifiers.Count == 0)
                {
                    prereleaseIdentifiers = ReadOnlyList<PrereleaseIdentifier>.Empty;
                    prereleaseString = "";
                }
                else if (prereleaseIdentifiers.Any(i => i == default))
                    throw new ArgumentException(PrereleaseIdentifierIsDefaultMessage, nameof(prerelease));
                else
                    prereleaseString = string.Join(".", prereleaseIdentifiers);
            }

            IReadOnlyList<MetadataIdentifier> metadataIdentifiers = null;
            string metadataString = null;
            if (metadata != null)
            {
                metadataIdentifiers = metadata.ToReadOnlyList();
                if (metadataIdentifiers.Count == 0)
                {
                    metadataIdentifiers = ReadOnlyList<MetadataIdentifier>.Empty;
                    metadataString = "";
                }
                else if (metadataIdentifiers.Any(i => i == default))
                    throw new ArgumentException(MetadataIdentifierIsDefaultMessage, nameof(metadata));
                else
                    metadataString = string.Join(".", metadataIdentifiers);
            }

            return new SemVersion(
                major ?? Major,
                minor ?? Minor,
                patch ?? Patch,
                prereleaseString ?? Prerelease,
                prereleaseIdentifiers ?? PrereleaseIdentifiers,
                metadataString ?? Metadata,
                metadataIdentifiers ?? MetadataIdentifiers);
        }

        /// <summary>
        /// Creates a copy of the current instance with multiple changed properties. Parses prerelease
        /// and metadata identifiers from dot separated strings. Use <see cref="With"/> instead if
        /// parsing is not needed. If changing only one property use one of the more specific
        /// <c>WithX()</c> methods.
        /// </summary>
        /// <param name="major">The value to replace the major version number or <see langword="null"/> to leave it unchanged.</param>
        /// <param name="minor">The value to replace the minor version number or <see langword="null"/> to leave it unchanged.</param>
        /// <param name="patch">The value to replace the patch version number or <see langword="null"/> to leave it unchanged.</param>
        /// <param name="prerelease">The value to replace the prerelease identifiers or <see langword="null"/> to leave it unchanged.</param>
        /// <param name="metadata">The value to replace the build metadata identifiers or <see langword="null"/> to leave it unchanged.</param>
        /// <param name="allowLeadingZeros">Allow leading zeros in numeric prerelease identifiers. Leading
        /// zeros will be removed.</param>
        /// <returns>The new version with changed properties.</returns>
        /// <exception cref="ArgumentOutOfRangeException">A <paramref name="major"/>,
        /// <paramref name="minor"/>, or <paramref name="patch"/> version number is negative.</exception>
        /// <exception cref="ArgumentException">A prerelease identifier is empty or contains invalid
        /// characters (i.e. characters that are not ASCII alphanumerics or hyphens) or has leading
        /// zeros for a numeric identifier when <paramref name="allowLeadingZeros"/> is
        /// <see langword="false"/>. Or, a metadata identifier is empty or contains invalid
        /// characters (i.e. characters that are not ASCII alphanumerics or hyphens).</exception>
        /// <exception cref="OverflowException">A numeric prerelease identifier value is too large
        /// for <see cref="int"/>.</exception>
        /// <remarks>
        /// The <see cref="WithParsedFrom"/> method is intended to be called using named argument
        /// syntax, passing only those fields to be changed.
        /// </remarks>
        /// <example>
        /// To change the patch version and prerelease identifiers version:
        /// <code>var modifiedVersion = version.WithParsedFrom(patch: 4, prerelease: "alpha.5");</code>
        /// </example>
        public SemVersion WithParsedFrom(
            int? major = null,
            int? minor = null,
            int? patch = null,
            string prerelease = null,
            string metadata = null,
            bool allowLeadingZeros = false)
        {
            // Note: It is tempting to null coalesce first, but then this method would report invalid
            // arguments on invalid SemVersion instances.
            if (major is int majorInt && majorInt < 0)
                throw new ArgumentOutOfRangeException(nameof(major), InvalidMajorVersionMessage);
            if (minor is int minorInt && minorInt < 0)
                throw new ArgumentOutOfRangeException(nameof(minor), InvalidMinorVersionMessage);
            if (patch is int patchInt && patchInt < 0)
                throw new ArgumentOutOfRangeException(nameof(patch), InvalidPatchVersionMessage);

            var prereleaseIdentifiers = prerelease?.SplitAndMapToReadOnlyList('.',
                i => new PrereleaseIdentifier(i, allowLeadingZeros, nameof(prerelease)));
            var metadataIdentifiers = metadata?.SplitAndMapToReadOnlyList('.',
                i => new MetadataIdentifier(i, nameof(metadata)));

            if (allowLeadingZeros && prerelease != null)
                // Leading zeros may have been removed, need to reconstruct the prerelease string
                prerelease = string.Join(".", prereleaseIdentifiers);

            return new SemVersion(
                major ?? Major,
                minor ?? Minor,
                patch ?? Patch,
                prerelease ?? Prerelease,
                prereleaseIdentifiers ?? PrereleaseIdentifiers,
                metadata ?? Metadata,
                metadataIdentifiers ?? MetadataIdentifiers);
        }

        #region With... Methods
        /// <summary>
        /// Creates a copy of the current instance with a different major version number.
        /// </summary>
        /// <param name="major">The value to replace the major version number.</param>
        /// <returns>The new version with the different major version number.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="major"/> is negative.</exception>
        public SemVersion WithMajor(int major)
        {
            if (major < 0) throw new ArgumentOutOfRangeException(nameof(major), InvalidMajorVersionMessage);
            if (Major == major) return this;
            return new SemVersion(major, Minor, Patch,
                Prerelease, PrereleaseIdentifiers, Metadata, MetadataIdentifiers);
        }

        /// <summary>
        /// Creates a copy of the current instance with a different minor version number.
        /// </summary>
        /// <param name="minor">The value to replace the minor version number.</param>
        /// <returns>The new version with the different minor version number.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="minor"/> is negative.</exception>
        public SemVersion WithMinor(int minor)
        {
            if (minor < 0) throw new ArgumentOutOfRangeException(nameof(minor), InvalidMinorVersionMessage);
            if (Minor == minor) return this;
            return new SemVersion(Major, minor, Patch,
                Prerelease, PrereleaseIdentifiers, Metadata, MetadataIdentifiers);
        }

        /// <summary>
        /// Creates a copy of the current instance with a different patch version number.
        /// </summary>
        /// <param name="patch">The value to replace the patch version number.</param>
        /// <returns>The new version with the different patch version number.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="patch"/> is negative.</exception>
        public SemVersion WithPatch(int patch)
        {
            if (patch < 0) throw new ArgumentOutOfRangeException(nameof(patch), InvalidPatchVersionMessage);
            if (Patch == patch) return this;
            return new SemVersion(Major, Minor, patch,
                Prerelease, PrereleaseIdentifiers, Metadata, MetadataIdentifiers);
        }

        /// <summary>
        /// Creates a copy of the current instance with a different prerelease portion.
        /// </summary>
        /// <param name="prerelease">The value to replace the prerelease portion.</param>
        /// <param name="allowLeadingZeros">Whether to allow leading zeros in the prerelease identifiers.
        /// If <see langword="true"/>, leading zeros will be allowed on numeric identifiers
        /// but will be removed.</param>
        /// <returns>The new version with the different prerelease identifiers.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="prerelease"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">A prerelease identifier is empty or contains invalid
        /// characters (i.e. characters that are not ASCII alphanumerics or hyphens) or has leading
        /// zeros for a numeric identifier when <paramref name="allowLeadingZeros"/> is <see langword="false"/>.</exception>
        /// <exception cref="OverflowException">A numeric prerelease identifier value is too large
        /// for <see cref="Int32"/>.</exception>
        /// <remarks>Because a valid numeric identifier does not have leading zeros, this constructor
        /// will never create a <see cref="PrereleaseIdentifier"/> with leading zeros even if
        /// <paramref name="allowLeadingZeros"/> is <see langword="true"/>. Any leading zeros will
        /// be removed.</remarks>
        public SemVersion WithPrereleaseParsedFrom(string prerelease, bool allowLeadingZeros = false)
        {
            if (prerelease is null) throw new ArgumentNullException(nameof(prerelease));
            if (prerelease.Length == 0) return WithoutPrerelease();
            var identifiers = prerelease.SplitAndMapToReadOnlyList('.',
                i => new PrereleaseIdentifier(i, allowLeadingZeros, nameof(prerelease)));
            if (allowLeadingZeros)
                // Leading zeros may have been removed, need to reconstruct the prerelease string
                prerelease = string.Join(".", identifiers);
            return new SemVersion(Major, Minor, Patch,
                prerelease, identifiers, Metadata, MetadataIdentifiers);
        }

        /// <summary>
        /// Creates a copy of the current instance with different prerelease identifiers.
        /// </summary>
        /// <param name="prereleaseIdentifier">The first identifier to replace the existing
        /// prerelease identifiers.</param>
        /// <param name="prereleaseIdentifiers">The rest of the identifiers to replace the
        /// existing prerelease identifiers.</param>
        /// <returns>The new version with the different prerelease identifiers.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="prereleaseIdentifier"/> or
        /// <paramref name="prereleaseIdentifiers"/> is <see langword="null"/> or one of the
        /// prerelease identifiers is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">A prerelease identifier is empty or contains invalid
        /// characters (i.e. characters that are not ASCII alphanumerics or hyphens) or has leading
        /// zeros for a numeric identifier.</exception>
        /// <exception cref="OverflowException">A numeric prerelease identifier value is too large
        /// for <see cref="Int32"/>.</exception>
        public SemVersion WithPrerelease(string prereleaseIdentifier, params string[] prereleaseIdentifiers)
        {
            if (prereleaseIdentifier is null) throw new ArgumentNullException(nameof(prereleaseIdentifier));
            if (prereleaseIdentifiers is null) throw new ArgumentNullException(nameof(prereleaseIdentifiers));
            var identifiers = prereleaseIdentifiers
                              .Prepend(prereleaseIdentifier)
                              .Select(i => new PrereleaseIdentifier(i, allowLeadingZeros: false, nameof(prereleaseIdentifiers)))
                              .ToReadOnlyList();
            return new SemVersion(Major, Minor, Patch,
                string.Join(".", identifiers), identifiers, Metadata, MetadataIdentifiers);
        }

        /// <summary>
        /// Creates a copy of the current instance with different prerelease identifiers.
        /// </summary>
        /// <param name="prereleaseIdentifiers">The identifiers to replace the prerelease identifiers.</param>
        /// <returns>The new version with the different prerelease identifiers.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="prereleaseIdentifiers"/> is
        /// <see langword="null"/> or one of the prerelease identifiers is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">A prerelease identifier is empty or contains invalid
        /// characters (i.e. characters that are not ASCII alphanumerics or hyphens) or has leading
        /// zeros for a numeric identifier.</exception>
        /// <exception cref="OverflowException">A numeric prerelease identifier value is too large
        /// for <see cref="Int32"/>.</exception>
        public SemVersion WithPrerelease(IEnumerable<string> prereleaseIdentifiers)
        {
            if (prereleaseIdentifiers is null) throw new ArgumentNullException(nameof(prereleaseIdentifiers));
            var identifiers = prereleaseIdentifiers
                              .Select(i => new PrereleaseIdentifier(i, allowLeadingZeros: false, nameof(prereleaseIdentifiers)))
                              .ToReadOnlyList();
            if (identifiers.Count == 0) return WithoutPrerelease();
            return new SemVersion(Major, Minor, Patch,
                string.Join(".", identifiers), identifiers, Metadata, MetadataIdentifiers);
        }

        /// <summary>
        /// Creates a copy of the current instance with different prerelease identifiers.
        /// </summary>
        /// <param name="prereleaseIdentifier">The first identifier to replace the existing
        /// prerelease identifiers.</param>
        /// <param name="prereleaseIdentifiers">The rest of the identifiers to replace the
        /// existing prerelease identifiers.</param>
        /// <returns>The new version with the different prerelease identifiers.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="prereleaseIdentifiers"/> is
        /// <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">A prerelease identifier has the default value.</exception>
        public SemVersion WithPrerelease(
                    PrereleaseIdentifier prereleaseIdentifier,
                    params PrereleaseIdentifier[] prereleaseIdentifiers)
        {
            if (prereleaseIdentifiers is null) throw new ArgumentNullException(nameof(prereleaseIdentifiers));
            var identifiers = prereleaseIdentifiers.Prepend(prereleaseIdentifier).ToReadOnlyList();
            if (identifiers.Any(i => i == default)) throw new ArgumentException(PrereleaseIdentifierIsDefaultMessage, nameof(prereleaseIdentifiers));
            return new SemVersion(Major, Minor, Patch,
                string.Join(".", identifiers), identifiers, Metadata, MetadataIdentifiers);
        }

        /// <summary>
        /// Creates a copy of the current instance with different prerelease identifiers.
        /// </summary>
        /// <param name="prereleaseIdentifiers">The identifiers to replace the prerelease identifiers.</param>
        /// <returns>The new version with the different prerelease identifiers.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="prereleaseIdentifiers"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">A prerelease identifier has the default value.</exception>
        public SemVersion WithPrerelease(IEnumerable<PrereleaseIdentifier> prereleaseIdentifiers)
        {
            if (prereleaseIdentifiers is null) throw new ArgumentNullException(nameof(prereleaseIdentifiers));
            var identifiers = prereleaseIdentifiers.ToReadOnlyList();
            if (identifiers.Count == 0) return WithoutPrerelease();
            if (identifiers.Any(i => i == default)) throw new ArgumentException(PrereleaseIdentifierIsDefaultMessage, nameof(prereleaseIdentifiers));
            return new SemVersion(Major, Minor, Patch,
                string.Join(".", identifiers), identifiers, Metadata, MetadataIdentifiers);
        }

        /// <summary>
        /// Creates a copy of the current instance without prerelease identifiers.
        /// </summary>
        /// <returns>The new version without prerelease identifiers.</returns>
        public SemVersion WithoutPrerelease()
        {
            if (!IsPrerelease) return this;
            return new SemVersion(Major, Minor, Patch,
                "", ReadOnlyList<PrereleaseIdentifier>.Empty, Metadata, MetadataIdentifiers);
        }

        /// <summary>
        /// Creates a copy of the current instance with different build metadata.
        /// </summary>
        /// <param name="metadata">The value to replace the build metadata.</param>
        /// <returns>The new version with the different build metadata.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="metadata"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">A metadata identifier is empty or contains invalid
        /// characters (i.e. characters that are not ASCII alphanumerics or hyphens).</exception>
        public SemVersion WithMetadataParsedFrom(string metadata)
        {
            if (metadata is null) throw new ArgumentNullException(nameof(metadata));
            if (metadata.Length == 0) return WithoutMetadata();
            var identifiers = metadata.SplitAndMapToReadOnlyList('.',
                i => new MetadataIdentifier(i, nameof(metadata)));
            return new SemVersion(Major, Minor, Patch,
                Prerelease, PrereleaseIdentifiers, metadata, identifiers);
        }

        /// <summary>
        /// Creates a copy of the current instance with different build metadata identifiers.
        /// </summary>
        /// <param name="metadataIdentifier">The first identifier to replace the existing
        /// build metadata identifiers.</param>
        /// <param name="metadataIdentifiers">The rest of the build metadata identifiers to replace the
        /// existing build metadata identifiers.</param>
        /// <returns>The new version with the different build metadata identifiers.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="metadataIdentifier"/> or
        /// <paramref name="metadataIdentifiers"/> is <see langword="null"/> or one of the metadata
        /// identifiers is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">A metadata identifier is empty or contains invalid
        /// characters (i.e. characters that are not ASCII alphanumerics or hyphens).</exception>
        public SemVersion WithMetadata(string metadataIdentifier, params string[] metadataIdentifiers)
        {
            if (metadataIdentifier is null) throw new ArgumentNullException(nameof(metadataIdentifiers));
            if (metadataIdentifiers is null) throw new ArgumentNullException(nameof(metadataIdentifiers));
            var identifiers = metadataIdentifiers
                              .Prepend(metadataIdentifier)
                              .Select(i => new MetadataIdentifier(i, nameof(metadataIdentifiers)))
                              .ToReadOnlyList();
            return new SemVersion(Major, Minor, Patch,
                Prerelease, PrereleaseIdentifiers, string.Join(".", identifiers), identifiers);
        }

        /// <summary>
        /// Creates a copy of the current instance with different build metadata identifiers.
        /// </summary>
        /// <param name="metadataIdentifiers">The identifiers to replace the build metadata identifiers.</param>
        /// <returns>The new version with the different build metadata identifiers.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="metadataIdentifiers"/> is
        /// <see langword="null"/> or one of the metadata identifiers is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">A metadata identifier is empty or contains invalid
        /// characters (i.e. characters that are not ASCII alphanumerics or hyphens).</exception>
        public SemVersion WithMetadata(IEnumerable<string> metadataIdentifiers)
        {
            if (metadataIdentifiers is null) throw new ArgumentNullException(nameof(metadataIdentifiers));
            var identifiers = metadataIdentifiers
                              .Select(i => new MetadataIdentifier(i, nameof(metadataIdentifiers)))
                              .ToReadOnlyList();
            if (identifiers.Count == 0) return WithoutMetadata();
            return new SemVersion(Major, Minor, Patch,
                Prerelease, PrereleaseIdentifiers, string.Join(".", identifiers), identifiers);
        }

        /// <summary>
        /// Creates a copy of the current instance with different build metadata identifiers.
        /// </summary>
        /// <param name="metadataIdentifier">The first identifier to replace the existing
        /// build metadata identifiers.</param>
        /// <param name="metadataIdentifiers">The rest of the identifiers to replace the
        /// existing build metadata identifiers.</param>
        /// <exception cref="ArgumentNullException"><paramref name="metadataIdentifiers"/> is
        /// <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">A metadata identifier has the default value.</exception>
        public SemVersion WithMetadata(
            MetadataIdentifier metadataIdentifier,
            params MetadataIdentifier[] metadataIdentifiers)
        {
            if (metadataIdentifiers is null) throw new ArgumentNullException(nameof(metadataIdentifiers));
            var identifiers = metadataIdentifiers.Prepend(metadataIdentifier).ToReadOnlyList();
            if (identifiers.Any(i => i == default))
                throw new ArgumentException(MetadataIdentifierIsDefaultMessage, nameof(metadataIdentifiers));
            return new SemVersion(Major, Minor, Patch,
                Prerelease, PrereleaseIdentifiers, string.Join(".", identifiers), identifiers);
        }

        /// <summary>
        /// Creates a copy of the current instance with different build metadata identifiers.
        /// </summary>
        /// <param name="metadataIdentifiers">The identifiers to replace the build metadata identifiers.</param>
        /// <returns>The new version with the different build metadata identifiers.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="metadataIdentifiers"/> is
        /// <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">A metadata identifier has the default value.</exception>
        public SemVersion WithMetadata(IEnumerable<MetadataIdentifier> metadataIdentifiers)
        {
            if (metadataIdentifiers is null) throw new ArgumentNullException(nameof(metadataIdentifiers));
            var identifiers = metadataIdentifiers.ToReadOnlyList();
            if (identifiers.Count == 0) return WithoutMetadata();
            if (identifiers.Any(i => i == default))
                throw new ArgumentException(MetadataIdentifierIsDefaultMessage, nameof(metadataIdentifiers));
            return new SemVersion(Major, Minor, Patch,
                Prerelease, PrereleaseIdentifiers, string.Join(".", identifiers), identifiers);
        }

        /// <summary>
        /// Creates a copy of the current instance without build metadata.
        /// </summary>
        /// <returns>The new version without build metadata.</returns>
        public SemVersion WithoutMetadata()
        {
            if (MetadataIdentifiers.Count == 0) return this;
            return new SemVersion(Major, Minor, Patch,
                Prerelease, PrereleaseIdentifiers, "", ReadOnlyList<MetadataIdentifier>.Empty);
        }

        /// <summary>
        /// Creates a copy of the current instance without prerelease identifiers or build metadata.
        /// </summary>
        /// <returns>The new version without prerelease identifiers or build metadata.</returns>
        public SemVersion WithoutPrereleaseOrMetadata()
        {
            if (!IsPrerelease && MetadataIdentifiers.Count == 0) return this;
            return new SemVersion(Major, Minor, Patch,
                "", ReadOnlyList<PrereleaseIdentifier>.Empty, "", ReadOnlyList<MetadataIdentifier>.Empty);
        }
        #endregion

        /// <summary>The major version number.</summary>
        /// <value>The major version number.</value>
        /// <remarks>An increase in the major version number indicates a backwards
        /// incompatible change.</remarks>
        public int Major { get; }

        /// <summary>The minor version number.</summary>
        /// <value>The minor version number.</value>
        /// <remarks>An increase in the minor version number indicates backwards
        /// compatible changes.</remarks>
        public int Minor { get; }

        /// <summary>The patch version number.</summary>
        /// <value>The patch version number.</value>
        /// <remarks>An increase in the patch version number indicates backwards
        /// compatible bug fixes.</remarks>
        public int Patch { get; }

        /// <summary>
        /// The prerelease identifiers for this version.
        /// </summary>
        /// <value>
        /// The prerelease identifiers for this version or empty string if this is a release version.
        /// </value>
        /// <include file='SemVersionDocParts.xml' path='docParts/part[@id="PrereleaseIdentifiers"]/*'/>
        // TODO v3.0.0 this should be null when there is no prerelease identifiers
        public string Prerelease { get; }

        /// <summary>
        /// The prerelease identifiers for this version.
        /// </summary>
        /// <value>
        /// The prerelease identifiers for this version or empty if this is a release version.
        /// </value>
        /// <include file='SemVersionDocParts.xml' path='docParts/part[@id="PrereleaseIdentifiers"]/*'/>
        public IReadOnlyList<PrereleaseIdentifier> PrereleaseIdentifiers { get; }

        /// <summary>Whether this is a prerelease version.</summary>
        /// <value>Whether this is a prerelease version. A semantic version with
        /// prerelease identifiers is a prerelease version.</value>
        /// <remarks>When this is <see langword="true"/>, the <see cref="Prerelease"/>
        /// and <see cref="PrereleaseIdentifiers"/> properties are non-empty. When
        /// this is <see langword="false"/>, the <see cref="Prerelease"/> property
        /// will be an empty string and the <see cref="PrereleaseIdentifiers"/> will
        /// be an empty collection.</remarks>
        public bool IsPrerelease => Prerelease.Length != 0;

        /// <summary>Whether this is a release version.</summary>
        /// <value>Whether this is a release version. A semantic version without
        /// prerelease identifiers is a release version.</value>
        /// <remarks>When this is <see langword="true"/>, the <see cref="Prerelease"/>
        /// property will be an empty string and the <see cref="PrereleaseIdentifiers"/>
        /// will be an empty collection. When this is <see langword="false"/>,
        /// the <see cref="Prerelease"/> and <see cref="PrereleaseIdentifiers"/>
        /// properties are non-empty.</remarks>
        public bool IsRelease => Prerelease.Length == 0;

        /// <summary>The build metadata for this version.</summary>
        /// <value>
        /// The build metadata for this version or empty string if there is no build metadata.
        /// </value>
        /// <include file='SemVersionDocParts.xml' path='docParts/part[@id="MetadataIdentifiers"]/*'/>
        [EditorBrowsable(EditorBrowsableState.Never), Obsolete("This property is obsolete. Use Metadata instead.")]
        public string Build => Metadata;

        /// <summary>The build metadata for this version.</summary>
        /// <value>The build metadata for this version or empty string if there
        /// is no metadata.</value>
        /// <include file='SemVersionDocParts.xml' path='docParts/part[@id="MetadataIdentifiers"]/*'/>
        // TODO v3.0.0 this should be null when there is no metadata
        public string Metadata { get; }

        /// <summary>The build metadata identifiers for this version.</summary>
        /// <value>The build metadata identifiers for this version or empty if there
        /// is no metadata.</value>
        /// <include file='SemVersionDocParts.xml' path='docParts/part[@id="MetadataIdentifiers"]/*'/>
        public IReadOnlyList<MetadataIdentifier> MetadataIdentifiers { get; }

        /// <summary>
        /// Converts this version to an equivalent string value.
        /// </summary>
        /// <returns>
        /// The <see cref="string" /> equivalent of this version.
        /// </returns>
        public override string ToString()
        {
            // Assume all separators ("..-+"), at most 2 extra chars
            var estimatedLength = 4 + Major.DecimalDigits()
                                    + Minor.DecimalDigits()
                                    + Patch.DecimalDigits()
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

        #region Equality
        /// <summary>
        /// Determines whether two semantic versions are equal.
        /// </summary>
        /// <returns><see langword="true"/> if the two values are equal, otherwise <see langword="false"/>.</returns>
        /// <remarks>Two versions are equal if every part of the version numbers are equal. Thus two
        /// versions with the same precedence may not be equal.</remarks>
        // TODO v3.0.0 rename parameters to `left` and `right` to be consistent with ComparePrecedence etc.
        public static bool Equals(SemVersion versionA, SemVersion versionB)
        {
            if (ReferenceEquals(versionA, versionB)) return true;
            if (versionA is null || versionB is null) return false;
            return versionA.Equals(versionB);
        }

        /// <summary>Determines whether the given object is equal to this version.</summary>
        /// <returns><see langword="true"/> if <paramref name="obj"/> is equal to the this version;
        /// otherwise <see langword="false"/>.</returns>
        /// <remarks>Two versions are equal if every part of the version numbers are equal. Thus two
        /// versions with the same precedence may not be equal.</remarks>
        public override bool Equals(object obj)
            => obj is SemVersion version && Equals(version);

        /// <summary>
        /// Determines whether two semantic versions are equal.
        /// </summary>
        /// <returns><see langword="true"/> if <paramref name="other"/> is equal to the this version;
        /// otherwise <see langword="false"/>.</returns>
        /// <remarks>Two versions are equal if every part of the version numbers are equal. Thus two
        /// versions with the same precedence may not be equal.</remarks>
        public bool Equals(SemVersion other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return Major == other.Major
                && Minor == other.Minor
                && Patch == other.Patch
                && string.Equals(Prerelease, other.Prerelease, StringComparison.Ordinal)
                && string.Equals(Metadata, other.Metadata, StringComparison.Ordinal);
        }

        /// <summary>
        /// Determines whether two semantic versions have the same precedence. Versions that differ
        /// only by build metadata have the same precedence.
        /// </summary>
        /// <param name="other">The semantic version to compare to.</param>
        /// <returns><see langword="true"/> if the version precedences are equal, otherwise
        /// <see langword="false"/>.</returns>
        public bool PrecedenceEquals(SemVersion other)
            => PrecedenceComparer.Compare(this, other) == 0;

        /// <summary>
        /// Determines whether two semantic versions have the same precedence. Versions that differ
        /// only by build metadata have the same precedence.
        /// </summary>
        /// <returns><see langword="true"/> if the version precedences are equal, otherwise
        /// <see langword="false"/>.</returns>
        public static bool PrecedenceEquals(SemVersion left, SemVersion right)
            => PrecedenceComparer.Compare(left, right) == 0;

        internal bool MajorMinorPatchEquals(SemVersion other)
        {
            if (other is null) return false;

            if (ReferenceEquals(this, other)) return true;

            return Major == other.Major
                   && Minor == other.Minor
                   && Patch == other.Patch;
        }

        /// <summary>
        /// Determines whether two semantic versions have the same precedence. Versions
        /// that differ only by build metadata have the same precedence.
        /// </summary>
        /// <param name="other">The semantic version to compare to.</param>
        /// <returns><see langword="true"/> if the version precedences are equal.</returns>
        [EditorBrowsable(EditorBrowsableState.Never), Obsolete("Method is obsolete. Use PrecedenceEquals() instead.")]
        public bool PrecedenceMatches(SemVersion other) => CompareByPrecedence(other) == 0;

        /// <summary>
        /// Gets a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms
        /// and data structures like a hash table.
        /// </returns>
        /// <remarks>Two versions are equal if every part of the version numbers are equal. Thus two
        /// versions with the same precedence may not have the same hash code.</remarks>
        public override int GetHashCode()
            => CombinedHashCode.Create(Major, Minor, Patch, Prerelease, Metadata);

        /// <summary>
        /// Determines whether two semantic versions are equal.
        /// </summary>
        /// <returns><see langword="true"/> if the two values are equal, otherwise <see langword="false"/>.</returns>
        /// <remarks>Two versions are equal if every part of the version numbers are equal. Thus two
        /// versions with the same precedence may not be equal.</remarks>
        public static bool operator ==(SemVersion left, SemVersion right) => Equals(left, right);

        /// <summary>
        /// Determines whether two semantic versions are <em>not</em> equal.
        /// </summary>
        /// <returns><see langword="true"/> if the two values are <em>not</em> equal, otherwise <see langword="false"/>.</returns>
        /// <remarks>Two versions are equal if every part of the version numbers are equal. Thus two
        /// versions with the same precedence may not be equal.</remarks>
        public static bool operator !=(SemVersion left, SemVersion right) => !Equals(left, right);
        #endregion

        #region Comparison
        /// <summary>
        /// An <see cref="IEqualityComparer{T}"/> and <see cref="IComparer{T}"/>
        /// that compares <see cref="SemVersion"/> by precedence. This can be used for sorting,
        /// binary search, and using <see cref="SemVersion"/> as a dictionary key.
        /// </summary>
        /// <value>A precedence comparer that implements <see cref="IEqualityComparer{T}"/> and
        /// <see cref="IComparer{T}"/> for <see cref="SemVersion"/>.</value>
        /// <include file='SemVersionDocParts.xml' path='docParts/part[@id="PrecedenceOrder"]/*'/>
        public static ISemVersionComparer PrecedenceComparer { get; } = Comparers.PrecedenceComparer.Instance;

        /// <summary>
        /// An <see cref="IEqualityComparer{T}"/> and <see cref="IComparer{T}"/>
        /// that compares <see cref="SemVersion"/> by sort order. This can be used for sorting,
        /// binary search, and using <see cref="SemVersion"/> as a dictionary key.
        /// </summary>
        /// <value>A sort order comparer that implements <see cref="IEqualityComparer{T}"/> and
        /// <see cref="IComparer{T}"/> for <see cref="SemVersion"/>.</value>
        /// <include file='SemVersionDocParts.xml' path='docParts/part[@id="SortOrder"]/*'/>
        public static ISemVersionComparer SortOrderComparer { get; } = Comparers.SortOrderComparer.Instance;

        /// <summary>
        /// Compares two versions and indicates whether this instance precedes, follows, or is in the same
        /// position as the other in the precedence order. Versions that differ only by build metadata
        /// have the same precedence.
        /// </summary>
        /// <returns>
        /// An integer that indicates whether this instance precedes, follows, or is in the same
        /// position as <paramref name="other"/> in the precedence order.
        /// <list type="table">
        ///     <listheader>
        ///         <term>Value</term>
        ///         <description>Condition</description>
        ///     </listheader>
        ///     <item>
        ///         <term>-1</term>
        ///         <description>This instance precedes <paramref name="other"/> in the precedence order.</description>
        ///     </item>
        ///     <item>
        ///         <term>0</term>
        ///         <description>This instance has the same precedence as <paramref name="other"/>.</description>
        ///     </item>
        ///     <item>
        ///         <term>1</term>
        ///         <description>
        ///             This instance follows <paramref name="other"/> in the precedence order
        ///             or <paramref name="other"/> is <see langword="null" />.
        ///         </description>
        ///     </item>
        /// </list>
        /// </returns>
        /// <include file='SemVersionDocParts.xml' path='docParts/part[@id="PrecedenceOrder"]/*'/>
        public int ComparePrecedenceTo(SemVersion other) => PrecedenceComparer.Compare(this, other);

        /// <summary>
        /// Compares two versions and indicates whether this instance precedes, follows, or is equal
        /// to the other in the sort order. Note that sort order is more specific than precedence order.
        /// </summary>
        /// <returns>
        /// An integer that indicates whether this instance precedes, follows, or is equal to the
        /// other in the sort order.
        /// <list type="table">
        /// 	<listheader>
        /// 		<term>Value</term>
        /// 		<description>Condition</description>
        /// 	</listheader>
        /// 	<item>
        /// 		<term>-1</term>
        /// 		<description>This instance precedes the other in the sort order.</description>
        /// 	</item>
        /// 	<item>
        /// 		<term>0</term>
        /// 		<description>This instance is equal to the other.</description>
        /// 	</item>
        /// 	<item>
        /// 		<term>1</term>
        /// 		<description>
        /// 			This instance follows the other in the sort order
        /// 			or the other is <see langword="null" />.
        /// 		</description>
        /// 	</item>
        /// </list>
        /// </returns>
        /// <include file='SemVersionDocParts.xml' path='docParts/part[@id="SortOrder"]/*'/>
        public int CompareSortOrderTo(SemVersion other) => SortOrderComparer.Compare(this, other);

        /// <summary>
        /// Compares two versions and indicates whether the first precedes, follows, or is in the same
        /// position as the second in the precedence order. Versions that differ only by build metadata
        /// have the same precedence.
        /// </summary>
        /// <returns>
        /// An integer that indicates whether <paramref name="left"/> precedes, follows, or is in the same
        /// position as <paramref name="right"/> in the precedence order.
        /// <list type="table">
        ///     <listheader>
        ///         <term>Value</term>
        ///         <description>Condition</description>
        ///     </listheader>
        ///     <item>
        ///         <term>-1</term>
        ///         <description>
        ///             <paramref name="left"/> precedes <paramref name="right"/> in the precedence
        ///             order or <paramref name="left"/> is <see langword="null" />.</description>
        ///     </item>
        ///     <item>
        ///         <term>0</term>
        ///         <description><paramref name="left"/> has the same precedence as <paramref name="right"/>.</description>
        ///     </item>
        ///     <item>
        ///         <term>1</term>
        ///         <description>
        ///             <paramref name="left"/> follows <paramref name="right"/> in the precedence order
        ///             or <paramref name="right"/> is <see langword="null" />.
        ///         </description>
        ///     </item>
        /// </list>
        /// </returns>
        /// <include file='SemVersionDocParts.xml' path='docParts/part[@id="PrecedenceOrder"]/*'/>
        public static int ComparePrecedence(SemVersion left, SemVersion right)
            => PrecedenceComparer.Compare(left, right);

        /// <summary>
        /// Compares two versions and indicates whether the first precedes, follows, or is equal to
        /// the second in the sort order. Note that sort order is more specific than precedence order.
        /// </summary>
        /// <returns>
        /// An integer that indicates whether <paramref name="left"/> precedes, follows, or is equal
        /// to <paramref name="right"/> in the sort order.
        /// <list type="table">
        ///     <listheader>
        ///         <term>Value</term>
        ///         <description>Condition</description>
        ///     </listheader>
        ///     <item>
        ///         <term>-1</term>
        ///         <description>
        ///             <paramref name="left"/> precedes <paramref name="right"/> in the sort
        ///             order or <paramref name="left"/> is <see langword="null" />.</description>
        ///     </item>
        ///     <item>
        ///         <term>0</term>
        ///         <description><paramref name="left"/> is equal to <paramref name="right"/>.</description>
        ///     </item>
        ///     <item>
        ///         <term>1</term>
        ///         <description>
        ///             <paramref name="left"/> follows <paramref name="right"/> in the sort order
        ///             or <paramref name="right"/> is <see langword="null" />.
        ///         </description>
        ///     </item>
        /// </list>
        /// </returns>
        /// <include file='SemVersionDocParts.xml' path='docParts/part[@id="SortOrder"]/*'/>
        public static int CompareSortOrder(SemVersion left, SemVersion right)
            => SortOrderComparer.Compare(left, right);

        /// <summary>
        /// Compares this version to an <see cref="Object"/> and indicates whether this instance
        /// precedes, follows, or is equal to the object in the sort order. Note that sort order
        /// is more specific than precedence order.
        /// </summary>
        /// <include file='SemVersionDocParts.xml' path='docParts/part[@id="CompareToReturns"]/*'/>
        /// <exception cref="InvalidCastException">The <paramref name="obj"/> is not a <see cref="SemVersion"/>.</exception>
        /// <include file='SemVersionDocParts.xml' path='docParts/part[@id="SortOrder"]/*'/>
        [EditorBrowsable(EditorBrowsableState.Never), Obsolete("Method is obsolete. Use CompareSortOrderTo() or ComparePrecedenceTo() instead.")]
        public int CompareTo(object obj) => CompareTo((SemVersion)obj);

        /// <summary>
        /// Compares two versions and indicates whether this instance precedes, follows, or is
        /// equal to the other in the sort order. Note that sort order is more specific than precedence order.
        /// </summary>
        /// <include file='SemVersionDocParts.xml' path='docParts/part[@id="CompareToReturns"]/*'/>
        /// <include file='SemVersionDocParts.xml' path='docParts/part[@id="SortOrder"]/*'/>
        [EditorBrowsable(EditorBrowsableState.Never), Obsolete("Method is obsolete. Use CompareSortOrderTo() or ComparePrecedenceTo() instead.")]
        public int CompareTo(SemVersion other)
        {
            var r = CompareByPrecedence(other);
            if (r != 0) return r;

            // If other is null, CompareByPrecedence() returns 1
            return CompareComponents(Metadata, other.Metadata);
        }

        /// <summary>
        /// Compares two versions and indicates whether this instance precedes, follows, or is in the same
        /// position as the other in the precedence order. Versions that differ only by build metadata
        /// have the same precedence.
        /// </summary>
        /// <returns>
        /// An integer that indicates whether this instance precedes, follows, or is in the same
        /// position as <paramref name="other"/> in the precedence order.
        /// <list type="table">
        ///     <listheader>
        ///         <term>Value</term>
        ///         <description>Condition</description>
        ///     </listheader>
        ///     <item>
        ///         <term>Less than zero</term>
        ///         <description>This instance precedes <paramref name="other"/> in the precedence order.</description>
        ///     </item>
        ///     <item>
        ///         <term>Zero</term>
        ///         <description>This instance has the same precedence as <paramref name="other"/>.</description>
        ///     </item>
        ///     <item>
        ///         <term>Greater than zero</term>
        ///         <description>
        ///             This instance follows <paramref name="other"/> in the precedence order
        ///             or <paramref name="other"/> is <see langword="null" />.
        ///         </description>
        ///     </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// <para>Precedence order is determined by comparing the major, minor, patch, and prerelease
        /// portion in order from left to right. Versions that differ only by build metadata have the
        /// same precedence. The major, minor, and patch version numbers are compared numerically. A
        /// prerelease version precedes a release version.</para>
        ///
        /// <para>The prerelease portion is compared by comparing each prerelease identifier from
        /// left to right. Numeric prerelease identifiers precede alphanumeric identifiers. Numeric
        /// identifiers are compared numerically. Alphanumeric identifiers are compared lexically
        /// in ASCII sort order. A longer series of prerelease identifiers follows a shorter series
        /// if all the preceding identifiers are equal.</para>
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never), Obsolete("Method is obsolete. Use ComparePrecedenceTo() or CompareSortOrderTo() instead.")]
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

            return CompareComponents(Prerelease, other.Prerelease, true);
        }

        [EditorBrowsable(EditorBrowsableState.Never), Obsolete]
        private static int CompareComponents(string a, string b, bool nonEmptyIsLower = false)
        {
            var aEmpty = string.IsNullOrEmpty(a);
            var bEmpty = string.IsNullOrEmpty(b);
            if (aEmpty && bEmpty)
                return 0;

            if (aEmpty)
                return nonEmptyIsLower ? 1 : -1;
            if (bEmpty)
                return nonEmptyIsLower ? -1 : 1;

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
        /// Compares two versions by sort order. Note that sort order is more specific than precedence order.
        /// </summary>
        /// <returns><see langword="true"/> if <paramref name="left"/> follows <paramref name="right"/>
        /// in the sort order; otherwise <see langword="false"/>.</returns>
        /// <include file='SemVersionDocParts.xml' path='docParts/part[@id="SortOrder"]/*'/>
        [EditorBrowsable(EditorBrowsableState.Never), Obsolete("Operator is obsolete. Use CompareSortOrder() or ComparePrecedence() instead.")]
        public static bool operator >(SemVersion left, SemVersion right)
            => Compare(left, right) > 0;

        /// <summary>
        /// Compares two versions by sort order. Note that sort order is more specific than precedence order.
        /// </summary>
        /// <returns><see langword="true"/> if <paramref name="left"/> follows or is equal to
        /// <paramref name="right"/> in the sort order; otherwise <see langword="false"/>.</returns>
        /// <include file='SemVersionDocParts.xml' path='docParts/part[@id="SortOrder"]/*'/>
        [EditorBrowsable(EditorBrowsableState.Never), Obsolete("Operator is obsolete. Use CompareSortOrder() or ComparePrecedence() instead.")]
        public static bool operator >=(SemVersion left, SemVersion right)
            => Equals(left, right) || Compare(left, right) > 0;

        /// <summary>
        /// Compares two versions by sort order. Note that sort order is more specific than precedence order.
        /// </summary>
        /// <returns><see langword="true"/> if <paramref name="left"/> precedes <paramref name="right"/>
        /// in the sort order; otherwise <see langword="false"/>.</returns>
        /// <include file='SemVersionDocParts.xml' path='docParts/part[@id="SortOrder"]/*'/>
        [EditorBrowsable(EditorBrowsableState.Never), Obsolete("Operator is obsolete. Use CompareSortOrder() or ComparePrecedence() instead.")]
        public static bool operator <(SemVersion left, SemVersion right)
            => Compare(left, right) < 0;

        /// <summary>
        /// Compares two versions by sort order. Note that sort order is more specific than precedence order.
        /// </summary>
        /// <returns><see langword="true"/> if <paramref name="left"/> precedes or is equal to
        /// <paramref name="right"/> in the sort order; otherwise <see langword="false"/>.</returns>
        /// <include file='SemVersionDocParts.xml' path='docParts/part[@id="SortOrder"]/*'/>
        [EditorBrowsable(EditorBrowsableState.Never), Obsolete("Operator is obsolete. Use CompareSortOrder() or ComparePrecedence() instead.")]
        public static bool operator <=(SemVersion left, SemVersion right)
            => Equals(left, right) || Compare(left, right) < 0;
        #endregion

        #region Ranges
        /// <summary>
        /// Checks if this version satisfies the predicate. Typically this is called with a
        /// <see cref="SemVersionRange"/> or <see cref="UnbrokenSemVersionRange"/>
        /// </summary>
        /// <param name="predicate">The predicate to evaluate. Commonly a
        /// <see cref="SemVersionRange"/> or <see cref="UnbrokenSemVersionRange"/>.</param>
        /// <returns><see langword="true"/> if the version is contained in the range,
        /// otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="predicate"/> is
        /// <see langword="null"/>.</exception>
        public bool Satisfies(Predicate<SemVersion> predicate)
        {
            if (predicate is null) throw new ArgumentNullException(nameof(predicate));
            return predicate(this);
        }

        // TODO add Satisfies overloads that take regular string ranges?

        /// <summary>
        /// Checks if this version is in the given range. Uses the same range syntax as npm.
        /// </summary>
        /// <remarks>
        /// It's more optimal to use the static parse methods on <see cref="NpmRangeSet"/>
        /// if you're going to be testing multiple versions against the same range
        /// to avoid having to parse the range multiple times.
        /// </remarks>
        /// <param name="range">The range to compare with.</param>
        /// <param name="includeAllPrerelease"></param>
        /// <returns><see langword="true"/> if the version is contained in the range,
        /// otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="range"/> is <see langword="null"/>.</exception>
        public bool SatisfiesNpm(string range, bool includeAllPrerelease = false)
        {
            if (range == null) throw new ArgumentNullException(nameof(range));

            var parsedRange = SemVersionRange.ParseNpm(range, includeAllPrerelease);
            return parsedRange.Contains(this);
        }
        #endregion

        /// <summary>
        /// Implicit conversion from <see cref="string"/> to <see cref="SemVersion"/>.
        /// </summary>
        /// <param name="version">The semantic version.</param>
        /// <returns>The <see cref="SemVersion"/> object.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="version"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The version number has an invalid format.</exception>
        /// <exception cref="OverflowException">The major, minor, or patch version number is larger than <see cref="int.MaxValue"/>.</exception>
        [EditorBrowsable(EditorBrowsableState.Never), Obsolete("Implicit conversion from string is obsolete. Use Parse() or TryParse() method instead.")]
        public static implicit operator SemVersion(string version)
            => Parse(version);
    }
}
