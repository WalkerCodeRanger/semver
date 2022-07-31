using System.Collections.Generic;
using Semver.Ranges;

namespace Semver.Comparers
{
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
            comparison = x.RightBound.CompareTo(y.RightBound);
            if (comparison != 0) return comparison;
            return x.IncludeAllPrerelease.CompareTo(y.IncludeAllPrerelease);
        }
    }
}
