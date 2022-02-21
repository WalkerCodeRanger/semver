using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
#if SERIALIZABLE
using System.Runtime.Serialization;
using System.Security.Permissions;
#endif
using System.Text.RegularExpressions;
using Semver.Utility;

namespace Semver
{
    /// <summary>
    /// A semantic version number.
    /// Conforms with v2.0.0 of semantic versioning (http://semver.org).
    /// </summary>
#if SERIALIZABLE
    [Serializable]
    public sealed class SemVersion : IComparable<SemVersion>, IComparable, IEquatable<SemVersion>, ISerializable
#else
    public sealed class SemVersion : IComparable<SemVersion>, IComparable, IEquatable<SemVersion>
#endif
    {
        internal const string InvalidSemVersionStylesMessage = "An invalid SemVersionStyles value was used.";
        private const string InvalidMajorVersionMessage = "Major version must be greater than or equal to zero.";
        private const string InvalidMinorVersionMessage = "Minor version must be greater than or equal to zero.";
        private const string InvalidPatchVersionMessage = "Patch version must be greater than or equal to zero.";
        private const string PrereleaseIdentifierIsDefaultMessage = "Prerelease identifier cannot be default/null.";
        private const string MetadataIdentifierIsDefaultMessage = "Metadata identifier cannot be default/null.";
        internal const int MaxVersionLength = 1024;

        /// <remarks>
        /// This exception is used with the <see cref="SemVersionParser.Parse"/>
        /// method to indicate parse failure without constructing a new exception.
        /// This exception should never be thrown or exposed outside of this
        /// package.
        /// </remarks>
        private static readonly Exception ParseFailedException = new Exception("Parse Failed");

        private static readonly Regex ParseEx =
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
            if (Prerelease.Length == 0)
                PrereleaseIdentifiers = ReadOnlyList<PrereleaseIdentifier>.Empty;
            else
                PrereleaseIdentifiers = Prerelease.Split('.')
#pragma warning disable CS0612 // Type or member is obsolete
                                                  .Select(PrereleaseIdentifier.CreateLoose)
#pragma warning restore CS0612 // Type or member is obsolete
                                                  .ToReadOnlyList();

            Metadata = build ?? "";
            if (Metadata.Length == 0)
                MetadataIdentifiers = ReadOnlyList<MetadataIdentifier>.Empty;
            else
                MetadataIdentifiers = Metadata.Split('.')
#pragma warning disable CS0612 // Type or member is obsolete
                                              .Select(MetadataIdentifier.CreateLoose)
#pragma warning restore CS0612 // Type or member is obsolete
                                              .ToReadOnlyList();
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="SemVersion"/> class from
        /// a <see cref="Version"/>.
        /// </summary>
        /// <param name="version">The <see cref="Version"/> that is used to initialize
        /// the Major, Minor, and Patch versions and the build metadata.</param>
        /// <returns>A <see cref="SemVersion"/> with the same Major and Minor version.
        /// The Patch version will be the fourth component of the version number. The
        /// build meta data will contain the third component of the version number if
        /// it is greater than zero.</returns>
        [Obsolete("This constructor is obsolete. Use SemVersion.FromVersion() instead.")]
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
        /// <paramref name="minor"/>, and <paramref name="patch"/> versions must not be
        /// negative. The <paramref name="prereleaseIdentifiers"/> and
        /// <paramref name="metadataIdentifiers"/> must not be null or contain invalid
        /// values and must be immutable.</remarks>
        internal SemVersion(int major, int minor, int patch,
            IReadOnlyList<PrereleaseIdentifier> prereleaseIdentifiers,
            IReadOnlyList<MetadataIdentifier> metadataIdentifiers)
        {
#if DEBUG
            if (major < 0) throw new ArgumentException(InvalidMajorVersionMessage, nameof(major));
            if (minor < 0) throw new ArgumentException(InvalidMinorVersionMessage, nameof(minor));
            if (patch < 0) throw new ArgumentException(InvalidPatchVersionMessage, nameof(patch));
            if (prereleaseIdentifiers is null) throw new ArgumentNullException(nameof(prereleaseIdentifiers));
            if (prereleaseIdentifiers.Any(i => i==default)) throw new ArgumentException(PrereleaseIdentifierIsDefaultMessage, nameof(prereleaseIdentifiers));
            if (metadataIdentifiers is null) throw new ArgumentNullException(nameof(metadataIdentifiers));
            if (metadataIdentifiers.Any(i => i == default)) throw new ArgumentException(MetadataIdentifierIsDefaultMessage, nameof(metadataIdentifiers));
#endif
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
        /// with a revision greater than zero cannot be converted to semantic versions. An
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
        /// <exception cref="InvalidOperationException">The semantic version is a prerelease version
        /// or has build metadata or has a negative major, minor, or patch version.</exception>
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

        // TODO Doc Comment
        public static SemVersion Parse(string version, SemVersionStyles style, int maxLength = MaxVersionLength)
        {
            if (!style.IsValid()) throw new ArgumentException(InvalidSemVersionStylesMessage, nameof(style));

            var ex = SemVersionParser.Parse(version, style, null, maxLength, out var semver);

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
        /// <exception cref="OverflowException">The Major, Minor, or Patch versions are larger than <c>int.MaxValue</c>.</exception>
        [Obsolete("Method is obsolete. Use Parse() overload with SemVersionStyles instead.")]
        public static SemVersion Parse(string version, bool strict = false)
        {
            var match = ParseEx.Match(version);
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

        // TODO Doc Comment
        // TODO MS guidelines say to always put out params last, perhaps the default maxLength should be an overload so it can come before the out param?
        public static bool TryParse(string version, SemVersionStyles style,
            out SemVersion semver, int maxLength = MaxVersionLength)
        {
            if (!style.IsValid()) throw new ArgumentException(InvalidSemVersionStylesMessage, nameof(style));
            var exception = SemVersionParser.Parse(version, style, ParseFailedException, maxLength, out semver);

            // This check ensures that ParseVersion doesn't construct an exception, but always returns ParseFailedException
            if (exception != null && exception != ParseFailedException)
                throw new InvalidOperationException($"{nameof(SemVersionParser)}.{nameof(SemVersionParser.Parse)} returned exception other than {nameof(ParseFailedException)}", exception);

            return exception is null;
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
        [Obsolete("Method is obsolete. Use TryParse() overload with SemVersionStyles instead.")]
#pragma warning disable RS0026 // Do not add multiple public overloads with optional parameters
        public static bool TryParse(string version, out SemVersion semver, bool strict = false)
#pragma warning restore RS0026 // Do not add multiple public overloads with optional parameters
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
        [Obsolete("Method is obsolete. Use With() or With...() method instead.")]
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
        /// Make a copy of the current instance with changed properties.
        /// </summary>
        /// <param name="major">The value to replace the major version or <see langword="null"/> to leave it unchanged.</param>
        /// <param name="minor">The value to replace the minor version or <see langword="null"/> to leave it unchanged.</param>
        /// <param name="patch">The value to replace the patch version or <see langword="null"/> to leave it unchanged.</param>
        /// <param name="prerelease">The value to replace the prerelease identifiers or <see langword="null"/> to leave it unchanged.</param>
        /// <param name="metadata">The value to replace the build metadata identifiers or <see langword="null"/> to leave it unchanged.</param>
        /// <param name="allowLeadingZeros">Allow leading zeros in numeric prerelease identifiers. Leading zeros will be trimmed.</param>
        /// <returns>The new version object.</returns>
        /// <remarks>
        /// The <see cref="With"/> method is intended to be called using named argument syntax, passing only
        /// those fields to be changed.
        /// </remarks>
        /// <example>
        /// To change only the patch version:
        /// <code>version.With(patch: 4)</code>
        /// </example>
        public SemVersion With(
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

            var prereleaseIdentifiers = prerelease?.Length == 0
                ? ReadOnlyList<PrereleaseIdentifier>.Empty
                : prerelease?.Split('.')
                             .Select(i => new PrereleaseIdentifier(i, allowLeadingZeros, nameof(prerelease)))
                             .ToReadOnlyList();
            var metadataIdentifiers = metadata?.Length == 0
                ? ReadOnlyList<MetadataIdentifier>.Empty
                : metadata?.Split('.')
                           .Select(i => new MetadataIdentifier(i, nameof(metadata)))
                           .ToReadOnlyList();

            return new SemVersion(
                major ?? Major,
                minor ?? Minor,
                patch ?? Patch,
                prereleaseIdentifiers ?? PrereleaseIdentifiers,
                metadataIdentifiers ?? MetadataIdentifiers);
        }

        #region With... Methods
        // TODO Doc Comment
        public SemVersion WithMajor(int major)
        {
            if (major < 0) throw new ArgumentOutOfRangeException(nameof(major), InvalidMajorVersionMessage);
            return new SemVersion(major, Minor, Patch, PrereleaseIdentifiers, MetadataIdentifiers);
        }

        // TODO Doc Comment
        public SemVersion WithMinor(int minor)
        {
            if (minor < 0) throw new ArgumentOutOfRangeException(nameof(minor), InvalidMinorVersionMessage);
            return new SemVersion(Major, minor, Patch, PrereleaseIdentifiers, MetadataIdentifiers);
        }

        // TODO Doc Comment
        public SemVersion WithPatch(int patch)
        {
            if (patch < 0) throw new ArgumentOutOfRangeException(nameof(patch), InvalidPatchVersionMessage);
            return new SemVersion(Major, Minor, patch, PrereleaseIdentifiers, MetadataIdentifiers);
        }

        // TODO Doc Comment
#pragma warning disable RS0026 // Do not add multiple public overloads with optional parameters
        public SemVersion WithPrerelease(string prerelease, bool allowLeadingZeros = false)
#pragma warning restore RS0026 // Do not add multiple public overloads with optional parameters
        {
            if (prerelease is null) throw new ArgumentNullException(nameof(prerelease));
            if (prerelease.Length == 0)
                return new SemVersion(Major, Minor, Patch, ReadOnlyList<PrereleaseIdentifier>.Empty, MetadataIdentifiers);
            var identifiers = prerelease.Split('.')
                              .Select(i => new PrereleaseIdentifier(i, allowLeadingZeros, nameof(prerelease)))
                              .ToReadOnlyList();
            return new SemVersion(Major, Minor, Patch, identifiers, MetadataIdentifiers);
        }

        // TODO Doc Comment
        public SemVersion WithPrerelease(params string[] prereleaseIdentifiers)
        {
            if (prereleaseIdentifiers is null) throw new ArgumentNullException(nameof(prereleaseIdentifiers));
            if (prereleaseIdentifiers.Length == 0)
                return new SemVersion(Major, Minor, Patch, ReadOnlyList<PrereleaseIdentifier>.Empty, MetadataIdentifiers);
            var identifiers = prereleaseIdentifiers
                              .Select(i => new PrereleaseIdentifier(i, allowLeadingZeros: false, nameof(prereleaseIdentifiers)))
                              .ToReadOnlyList();

            return new SemVersion(Major, Minor, Patch, identifiers, MetadataIdentifiers);
        }

        // TODO Doc Comment
#pragma warning disable RS0026 // Do not add multiple public overloads with optional parameters
        public SemVersion WithPrerelease(IEnumerable<string> prereleaseIdentifiers, bool allowLeadingZeros = false)
#pragma warning restore RS0026 // Do not add multiple public overloads with optional parameters
        {
            if (prereleaseIdentifiers is null) throw new ArgumentNullException(nameof(prereleaseIdentifiers));
            var identifiers = prereleaseIdentifiers
                              .Select(i => new PrereleaseIdentifier(i, allowLeadingZeros, nameof(prereleaseIdentifiers)))
                              .ToReadOnlyList();
            return new SemVersion(Major, Minor, Patch, identifiers, MetadataIdentifiers);
        }

        // TODO Doc Comment
        public SemVersion WithPrerelease(IEnumerable<PrereleaseIdentifier> prereleaseIdentifiers)
        {
            if (prereleaseIdentifiers is null) throw new ArgumentNullException(nameof(prereleaseIdentifiers));
            var identifiers = prereleaseIdentifiers.ToReadOnlyList();
            if (identifiers.Any(i => i == default)) throw new ArgumentException(PrereleaseIdentifierIsDefaultMessage, nameof(prereleaseIdentifiers));
            return new SemVersion(Major, Minor, Patch, identifiers, MetadataIdentifiers);
        }

        // TODO Doc Comment
        public SemVersion WithoutPrerelease()
            => new SemVersion(Major, Minor, Patch, ReadOnlyList<PrereleaseIdentifier>.Empty, MetadataIdentifiers);

        // TODO Doc Comment
        public SemVersion WithMetadata(string metadata)
        {
            if (metadata is null) throw new ArgumentNullException(nameof(metadata));
            if (metadata.Length == 0)
                return new SemVersion(Major, Minor, Patch, PrereleaseIdentifiers, ReadOnlyList<MetadataIdentifier>.Empty);
            var identifiers = metadata.Split('.')
                                      .Select(i => new MetadataIdentifier(i, nameof(metadata)))
                                      .ToReadOnlyList();
            return new SemVersion(Major, Minor, Patch, PrereleaseIdentifiers, identifiers);
        }

        // TODO Doc Comment
        public SemVersion WithMetadata(params string[] metadataIdentifiers)
        {
            if (metadataIdentifiers is null) throw new ArgumentNullException(nameof(metadataIdentifiers));
            if (metadataIdentifiers.Length == 0)
                return new SemVersion(Major, Minor, Patch, PrereleaseIdentifiers, ReadOnlyList<MetadataIdentifier>.Empty);
            var identifiers = metadataIdentifiers
                              .Select(i => new MetadataIdentifier(i, nameof(metadataIdentifiers)))
                              .ToReadOnlyList();
            return new SemVersion(Major, Minor, Patch, PrereleaseIdentifiers, identifiers);
        }

        // TODO Doc Comment
        public SemVersion WithMetadata(IEnumerable<string> metadataIdentifiers)
        {
            if (metadataIdentifiers is null) throw new ArgumentNullException(nameof(metadataIdentifiers));
            var identifiers = metadataIdentifiers
                              .Select(i => new MetadataIdentifier(i, nameof(metadataIdentifiers)))
                              .ToReadOnlyList();
            return new SemVersion(Major, Minor, Patch, PrereleaseIdentifiers, identifiers);
        }

        // TODO Doc Comment
        public SemVersion WithMetadata(IEnumerable<MetadataIdentifier> metadataIdentifiers)
        {
            if (metadataIdentifiers is null) throw new ArgumentNullException(nameof(metadataIdentifiers));
            var identifiers = metadataIdentifiers.ToReadOnlyList();
            if (identifiers.Any(i => i == default))
                throw new ArgumentException(MetadataIdentifierIsDefaultMessage, nameof(metadataIdentifiers));
            return new SemVersion(Major, Minor, Patch, PrereleaseIdentifiers, identifiers);
        }

        // TODO Doc Comment
        public SemVersion WithoutMetadata() =>
            new SemVersion(Major, Minor, Patch, PrereleaseIdentifiers, ReadOnlyList<MetadataIdentifier>.Empty);

        // TODO Doc Comment
        public SemVersion WithoutPrereleaseOrMetadata() =>
            new SemVersion(Major, Minor, Patch, ReadOnlyList<PrereleaseIdentifier>.Empty, ReadOnlyList<MetadataIdentifier>.Empty);
        #endregion

        /// <value>
        /// The major version.
        /// </value>
        public int Major { get; }

        /// <value>
        /// The minor version.
        /// </value>
        public int Minor { get; }

        /// <value>
        /// The patch version.
        /// </value>
        public int Patch { get; }

        /// <value>
        /// The prerelease identifiers or empty string if this is a release version.
        /// </value>
        /// <remarks>
        /// A prerelease version label follows the main version number separated
        /// by a dash ('-'). It is a series of identifiers each of which may either
        /// be alphanumeric or numeric. Prerelease versions have lower precedence
        /// than release versions.
        /// </remarks>
        public string Prerelease { get; }

        // TODO Doc Comment
        public IReadOnlyList<PrereleaseIdentifier> PrereleaseIdentifiers { get; }

        /// <value>Whether this is a prerelease version. <see langword="true"/>
        /// if the <see cref="Prerelease"/> property is non-empty; <see langword="false"/>
        /// if it is empty.</value>
        public bool IsPrerelease => Prerelease.Length != 0;

        /// <value>
        /// The build metadata or empty string if there is no build metadata.
        /// </value>
        [Obsolete("This property is obsolete. Use Metadata instead.")]
        public string Build => Metadata;

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

        // TODO Doc Comment
        public IReadOnlyList<MetadataIdentifier> MetadataIdentifiers { get; }

        /// <summary>
        /// Converts this version to a <see cref="string" />.
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

            // If other is null, CompareByPrecedence() returns 1
            return CompareComponent(Metadata, other.Metadata);
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
        public override bool Equals(object obj)
        {
            return obj is SemVersion version && Equals(version);
        }

        /// <summary>
        /// Indicates whether the <see cref="SemVersion"/> is equal to another <see cref="SemVersion"/>.
        /// </summary>
        /// <param name="other">An version to compare with this object.</param>
        /// <returns>
        ///   <see langword="true"/> if the current version is equal to the <paramref name="other"/> parameter, otherwise <see langword="false"/>.
        /// </returns>
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
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms
        /// and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
            => HashCodes.Combine(Major, Minor, Patch, Prerelease, Metadata);

#if SERIALIZABLE
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
        /// Implicit conversion from <see cref="string"/> to <see cref="SemVersion"/>.
        /// </summary>
        /// <param name="version">The semantic version.</param>
        /// <returns>The <see cref="SemVersion"/> object.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="version"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The version number has an invalid format.</exception>
        /// <exception cref="OverflowException">The Major, Minor, or Patch versions are larger than <c>int.MaxValue</c>.</exception>
        [Obsolete("Implicit conversion from string is obsolete. Use Parse() or TryParse() method instead.")]
        public static implicit operator SemVersion(string version)
            => Parse(version);

        /// <summary>
        /// Compares two semantic versions for equality.
        /// </summary>
        /// <param name="left">The left value.</param>
        /// <param name="right">The right value.</param>
        /// <returns>If left is equal to right <see langword="true"/>, otherwise <see langword="false"/>.</returns>
        public static bool operator ==(SemVersion left, SemVersion right)
            => Equals(left, right);

        /// <summary>
        /// Compares two semantic versions for inequality.
        /// </summary>
        /// <param name="left">The left value.</param>
        /// <param name="right">The right value.</param>
        /// <returns>If left is not equal to right <see langword="true"/>, otherwise <see langword="false"/>.</returns>
        public static bool operator !=(SemVersion left, SemVersion right)
            => !Equals(left, right);

        /// <summary>
        /// Compares two semantic versions.
        /// </summary>
        /// <param name="left">The left value.</param>
        /// <param name="right">The right value.</param>
        /// <returns>If left is greater than right <see langword="true"/>, otherwise <see langword="false"/>.</returns>
        public static bool operator >(SemVersion left, SemVersion right)
            => Compare(left, right) > 0;

        /// <summary>
        /// Compares two semantic versions.
        /// </summary>
        /// <param name="left">The left value.</param>
        /// <param name="right">The right value.</param>
        /// <returns>If left is greater than or equal to right <see langword="true"/>, otherwise <see langword="false"/>.</returns>
        public static bool operator >=(SemVersion left, SemVersion right)
            => Equals(left, right) || Compare(left, right) > 0;

        /// <summary>
        /// Compares two semantic versions.
        /// </summary>
        /// <param name="left">The left value.</param>
        /// <param name="right">The right value.</param>
        /// <returns>If left is less than right <see langword="true"/>, otherwise <see langword="false"/>.</returns>
        public static bool operator <(SemVersion left, SemVersion right)
            => Compare(left, right) < 0;

        /// <summary>
        /// Compares two semantic versions.
        /// </summary>
        /// <param name="left">The left value.</param>
        /// <param name="right">The right value.</param>
        /// <returns>If left is less than or equal to right <see langword="true"/>, otherwise <see langword="false"/>.</returns>
        public static bool operator <=(SemVersion left, SemVersion right)
            => Equals(left, right) || Compare(left, right) < 0;
    }
}
