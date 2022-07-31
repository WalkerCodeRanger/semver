using Semver.Comparers;
using Semver.Ranges;
using Xunit;

namespace Semver.Test.Comparers
{
    public class UnbrokenSemVersionRangeComparerTests
    {
        private static readonly UnbrokenSemVersionRangeComparer Comparer = UnbrokenSemVersionRangeComparer.Instance;

        [Fact]
        public void CompareInclusivenessOfStart()
        {
            var inclusive = UnbrokenSemVersionRange.AtLeast(new SemVersion(1, 2, 3));
            var exclusive = UnbrokenSemVersionRange.GreaterThan(new SemVersion(1, 2, 3));

            var comparison = Comparer.Compare(inclusive, exclusive);

            Assert.Equal(-1, comparison);
        }

        [Fact]
        public void CompareInclusivenessOfEnd()
        {
            var inclusive = UnbrokenSemVersionRange.AtMost(new SemVersion(1, 2, 3));
            var exclusive = UnbrokenSemVersionRange.LessThan(new SemVersion(1, 2, 3));

            var comparison = Comparer.Compare(exclusive, inclusive);

            Assert.Equal(-1, comparison);
        }
    }
}
