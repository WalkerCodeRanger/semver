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
        /// Accept version strings strictly conforming to the latest supported SemVer spec.
        /// Currently, that is SemVer 2.0
        /// </summary>
        Strict = 0,

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
        /// Accept any version string format supported.
        /// </summary>
        /// <remarks>This is a composite semantic version style.
        ///
        /// The formats accepted by this style will change when more formats are supported.</remarks>
        Any = unchecked((int)0xFFFF_FFFF),
    }
}
