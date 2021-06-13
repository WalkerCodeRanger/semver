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
        /// This is a derived semantic version style.
        ///
        /// Version numbers with a trailing dash but no prerelease version (e.g. "1.2.3-")
        /// are not accepted even though they appear to be supported by the spec.
        ///
        /// The SemVer 1.0 spec states that "A pre-release version number MAY be denoted by appending
        /// an arbitrary string immediately following the patch version and a dash. The string MUST
        /// be comprised of only alphanumerics plus dash [0-9A-Za-z-]." It states no length
        /// requirements on the "arbitrary string." A strict interpretation of this would indicate
        /// that "1.2.3-" was a valid SemVer 1.0 version string.
        ///
        /// However, it is unclear whether "1.2.3-" should be interpreted as a prerelease version
        /// with lower precedence than "1.2.3" or as equivalent to it. It would seem to be the former
        /// because it has "an arbitrary string immediately following the patch version and a dash."
        /// Allowing prerelease versions with no prerelease identifiers would make the API more
        /// complex and confusing for a very rare edge case. For example, it might mean that
        /// release versions had to be represented with a null prerelease identifiers list. Because
        /// of this, it was decided not to support such versions.
        /// </remarks>
        SemVer1 = AllowLeadingZeros,

        /// <summary>
        /// Accept version strings as defined by the SemVer 2.0 spec
        /// </summary>
        /// <remarks>This is a composite semantic version style.</remarks>
        SemVer2 = AllowMultiplePrereleaseIdentifiers | AllowMetadata,

        /// <summary>
        /// Accept version strings strictly conforming to the latest supported SemVer spec
        /// </summary>
        /// <remarks>This is a composite semantic version style.
        ///
        /// The enum value and formats accepted by this style will change when new SemVer specs are
        /// supported.</remarks>
        Strict = SemVer2,

        /// <summary>
        /// Allow leading zeros on major, minor, patch, and prerelease version numbers
        /// </summary>
        AllowLeadingZeros = 1,

        /// <summary>
        /// Allow leading whitespace. When combined with leading 'v', the whitespace
        /// must come before the 'v'
        /// </summary>
        AllowLeadingWhitespace = 1 << 1,

        /// <summary>
        /// Allow trailing whitespace
        /// </summary>
        AllowTrailingWhitespace = 1 << 2,

        /// <summary>
        /// Allow leading and/or trailing whitespace. When combined with leading 'v',
        /// the leading whitespace must come before the 'v'
        /// </summary>
        AllowWhitespace = AllowLeadingWhitespace | AllowTrailingWhitespace,

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
        /// Allow multiple prerelease identifiers
        /// </summary>
        AllowMultiplePrereleaseIdentifiers = 1 << 7,

        /// <summary>
        /// Allow a build metadata section
        /// </summary>
        AllowMetadata = 1 << 8,

        /// <summary>
        /// Accept any version string format supported.
        /// </summary>
        /// <remarks>This is a composite semantic version style.
        ///
        /// The formats accepted by this style will change when more formats are supported.</remarks>
        Any = unchecked((int)0xFFFF_FFFF),
    }
}
