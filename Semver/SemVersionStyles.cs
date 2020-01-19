using System;

namespace Semver
{
    /// <summary>
    /// Determines the styles that are allowed in version strings passed to the
    /// <code>Parse</code> and <code>TryParse</code> methods of <see cref="SemVersion"/>.
    /// </summary>
    [Flags]
    public enum SemVersionStyles
    {
        /// <summary>
        /// Accept version strings as defined by the SemVer 1.0 spec
        /// </summary>
        /// <remarks>
        /// This is a composite semantic version style.
        ///
        /// Note: Version numbers with a trailing dash but no prerelease version (e.g. "1.2.3-")
        /// are not accepted event though they are supported by the spec.</remarks>
        SemVer1 = AllowLeadingZeros | DisallowMetadata | DisallowMultiplePrereleaseIdentifiers,

        /// <summary>
        /// Accept version strings as defined by the SemVer 2.0 spec
        /// </summary>
        /// <remarks>This is a composite semantic version style.</remarks>
        SemVer2 = 0,

        /// <summary>
        /// Accept version strings strictly conforming to the latest supported SemVer spec
        /// </summary>
        /// <remarks>This is a composite semantic version style.
        ///
        /// The formats accepted by this style will change when new SemVer specs are
        /// supported.</remarks>
        Strict = SemVer2,

        /// <summary>
        /// Accept any version string format supported.
        /// </summary>
        /// <remarks>This is a composite semantic version style.
        ///
        /// The formats accepted by this style will change when more formats are supported.</remarks>
        Any = AllowLeadingWhite | AllowTrailingWhite | AllowV | OptionalMinorPatch | AllowLeadingZeros,

        /// <summary>
        /// Allow leading zeros on major, minor, and prerelease version numbers
        /// </summary>
        AllowLeadingZeros = 1,

        /// <summary>
        /// Allow leading whitespace. When combined with leading 'v', the whitespace
        /// must come before the 'v'
        /// </summary>
        AllowLeadingWhite = 1 << 1,

        /// <summary>
        /// Allow trailing whitespace
        /// </summary>
        AllowTrailingWhite = 1 << 2,

        /// <summary>
        /// Allow a leading lowercase 'v'
        /// </summary>
        AllowLowerV = 1 << 3,

        /// <summary>
        /// Allow a leading uppercase 'V'
        /// </summary>
        AllowUpperV = 1 << 4,

        /// <summary>
        /// Allow a leading 'v' or 'V'
        /// </summary>
        AllowV = AllowLowerV | AllowUpperV,

        /// <summary>
        /// Patch version number is optional
        /// </summary>
        OptionalPatch = 1 << 5,

        /// <summary>
        /// Minor and patch version numbers are optional
        /// </summary>
        OptionalMinorPatch = 1 << 6 | OptionalPatch,

        /// <summary>
        /// Disallow multiple prerelease identifiers
        /// </summary>
        DisallowMultiplePrereleaseIdentifiers = 1 << 7,

        /// <summary>
        /// Disallow a build metadata section
        /// </summary>
        DisallowMetadata = 1 << 8,
    }
}
