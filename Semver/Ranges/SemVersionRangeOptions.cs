using System;

namespace Semver.Ranges
{
    [Flags]
    public enum SemVersionRangeOptions
    {
        #region Matching SemVersionStyles
        /// <summary>
        /// Accept version strings strictly conforming to the SemVer 2.0 spec without metadata.
        /// </summary>
        Strict = 0,

        /// <summary>
        /// <para>Allow leading zeros on major, minor, patch, and prerelease version numbers.</para>
        ///
        /// <para>Leading zeros will be removed from the constructed version number.</para>
        /// </summary>
        AllowLeadingZeros = 1,

        /// <summary>
        /// Allow a leading lowercase "v".
        /// </summary>
        AllowLowerV = 1 << 3,

        /// <summary>
        /// Allow a leading uppercase "V".
        /// </summary>
        AllowUpperV = 1 << 4,

        /// <summary>
        /// Allow a leading "v" or "V".
        /// </summary>
        AllowV = AllowLowerV | AllowUpperV,

        /// <summary>
        /// Patch version number is optional.
        /// </summary>
        OptionalPatch = 1 << 5,

        /// <summary>
        /// Minor and patch version numbers are optional.
        /// </summary>
        OptionalMinorPatch = 1 << 6 | OptionalPatch,
        #endregion

        #region Useing values of SemVersionStyles that do not apply to ranges
        IncludeAllPrerelease = 1 << 1,
        AllowMetadata = 1 << 2,
        #endregion

        Loose = AllowLeadingZeros | AllowV | OptionalMinorPatch | AllowMetadata,
    }
}
