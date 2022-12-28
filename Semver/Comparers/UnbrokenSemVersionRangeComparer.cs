using System.Collections.Generic;
using Semver.Ranges;

namespace Semver.Comparers
{
    /// <summary>
    /// Compare <see cref="UnbrokenSemVersionRange"/> by the left bound and then by the reversed
    /// right bound. Thus wider ranges sort before narrower ones. Finally, sort ranges including
    /// prerelease before those not including prerelease.
    /// </summary>
    /// <remarks>This order is important to the removal of fully contained ranges from
    /// <see cref="SemVersionRange"/> since it sorts the ranges and then checks earlier ranges to
    /// see if they contain later ranges. Thus, more inclusive ranges need to come first so that
    /// the ranges they contain will be removed. Note that the <see cref="SemVersionRange"/> API
    /// will never expose this order exactly because contained ranges will not be included in the
    /// final <see cref="SemVersionRange"/>.</remarks>
    internal sealed class UnbrokenSemVersionRangeComparer : Comparer<UnbrokenSemVersionRange>
    {
        #region Singleton
        public static readonly UnbrokenSemVersionRangeComparer Instance = new UnbrokenSemVersionRangeComparer();

        private UnbrokenSemVersionRangeComparer() { }
        #endregion

        public override int Compare(UnbrokenSemVersionRange x, UnbrokenSemVersionRange y)
        {
            var comparison = x.LeftBound.CompareTo(y.LeftBound);
            if (comparison != 0) return comparison;
            comparison = -x.RightBound.CompareTo(y.RightBound);
            if (comparison != 0) return comparison;
            return -x.IncludeAllPrerelease.CompareTo(y.IncludeAllPrerelease);
        }
    }
}
