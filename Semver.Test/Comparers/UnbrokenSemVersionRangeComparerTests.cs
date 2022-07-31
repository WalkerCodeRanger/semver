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

            Assert.Equal(-1, Comparer.Compare(inclusive, exclusive));
            Assert.Equal(1, Comparer.Compare(exclusive, inclusive));
        }

        [Fact]
        public void CompareInclusivenessOfEnd()
        {
            var inclusive = UnbrokenSemVersionRange.AtMost(new SemVersion(1, 2, 3));
            var exclusive = UnbrokenSemVersionRange.LessThan(new SemVersion(1, 2, 3));

            Assert.Equal(-1, Comparer.Compare(exclusive, inclusive));
            Assert.Equal(1, Comparer.Compare(inclusive, exclusive));
        }

        [Fact]
        public void CompareByIncludePrerelease()
        {
            var start = new SemVersion(1, 2, 3);
            var end = new SemVersion(1, 3, 6);
            var release = UnbrokenSemVersionRange.Inclusive(start, end, false);
            var prerelease = UnbrokenSemVersionRange.Inclusive(start, end, true);

            Assert.Equal(-1, Comparer.Compare(release, prerelease));
            Assert.Equal(1, Comparer.Compare(prerelease, release));
        }
    }
}
