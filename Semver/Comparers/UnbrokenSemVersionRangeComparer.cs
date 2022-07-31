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
            var comparison = Compare(x.LeftBound, y.LeftBound);
            if (comparison != 0) return comparison;
            comparison = Compare(x.RightBound, y.RightBound);
            if (comparison != 0) return comparison;
            return x.IncludeAllPrerelease.CompareTo(y.IncludeAllPrerelease);
        }

        private static int Compare(LeftBoundedRange x, LeftBoundedRange y)
        {
            var comparison = PrecedenceComparer.Instance.Compare(x.Version, y.Version);
            if (comparison != 0) return comparison;
            return -x.Inclusive.CompareTo(y.Inclusive);
        }

        private static int Compare(RightBoundedRange x, RightBoundedRange y)
        {
            var comparison = PrecedenceComparer.Instance.Compare(x.Version, y.Version);
            if (comparison != 0) return comparison;
            return x.Inclusive.CompareTo(y.Inclusive);
        }
    }
}
