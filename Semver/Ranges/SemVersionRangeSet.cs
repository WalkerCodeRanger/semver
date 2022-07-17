using Semver.Ranges.Npm;

namespace Semver.Ranges
{
    /// <summary>
    /// A collection of ranges of <see cref="SemVersion"/>. Allows for testing whether a version is
    /// contained in those ranges. Logically, it is as if the ranges are "or'd" together.
    /// </summary>
    /// <remarks>
    /// A range of <see cref="SemVersion"/> is not a simple range of versions because of prerelease
    /// versions. Many ranges don't include and prerelease versions even if they are between the
    /// ends of the range. Ranges that do allow prerelease versions often don't allow them across
    /// the whole range but rather only around specific prerelease versions where they have been
    /// opted into.
    /// </remarks>
    public abstract class SemVersionRangeSet
    {
        private protected SemVersionRangeSet() { }

        /// <summary>
        /// Returns whether this range set contains the specified version.
        /// </summary>
        /// <param name="version">The version to check if it's contained within this range.</param>
        /// <returns>True if this range contains the specified version.</returns>
        public abstract bool Contains(SemVersion version);

        public static SemVersionRangeSet ParseNpm(string range, bool includeAllPrerelease = false)
            => NpmRange.Parse(range, includeAllPrerelease);

        public static bool TryParseNpm(string range, bool includeAllPrerelease, out SemVersionRangeSet ranges)
        {
            var success = NpmRange.TryParse(range, includeAllPrerelease, out var npmRange);
            ranges = npmRange;
            return success;
        }

        public static bool TryParseNpm(string range, out SemVersionRangeSet ranges)
        {
            var success = NpmRange.TryParse(range, false, out var npmRange);
            ranges = npmRange;
            return success;
        }
    }
}