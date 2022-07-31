using Semver.Ranges;
using Semver.Test.Builders;
using Xunit;

namespace Semver.Test.Ranges
{
    public class UnbrokenSemVersionRangeOverlapsTests
    {
        // Note: must be declared before use because of order of static initializers
        public static readonly UnbrokenSemVersionRange Empty = UnbrokenSemVersionRange.Empty;
        public static readonly UnbrokenSemVersionRange All = UnbrokenSemVersionRange.All;
        public static readonly UnbrokenSemVersionRange AllRelease = UnbrokenSemVersionRange.AllRelease;

        public static readonly TheoryData<RangeOverlapsTestCase> RangeOverlapsTestCases = new TheoryData<RangeOverlapsTestCase>()
        {
            NoOverlap(Empty, Empty),
            NoOverlap(Empty, All),
            NoOverlap(Empty, AllRelease),
            Overlaps(All, All),
            Overlaps(AllRelease, AllRelease),
            NoOverlap(Inclusive("1.2.3", "4.5.6"), Inclusive("7.8.9", "8.2.3")),
            Overlaps(Inclusive("1.2.3", "4.5.6"), Inclusive("4.5.6", "7.8.9")),
            NoOverlap(InclusiveOfStart("1.2.3", "4.5.6"), InclusiveOfEnd("4.5.6", "7.8.9")),
            NoOverlap(Inclusive("1.2.3", "4.5.6"), InclusiveOfEnd("4.5.6", "7.8.9")),
            NoOverlap(InclusiveOfStart("1.2.3", "4.5.6"), Inclusive("4.5.6", "7.8.9")),
            Overlaps(Inclusive("1.2.3", "4.5.6"), Inclusive("1.5.0", "3.5.4")),
        };

        [Theory]
        [MemberData(nameof(RangeOverlapsTestCases))]
        public void OverlapsIsCorrect(RangeOverlapsTestCase testCase)
        {
            if (testCase.Overlaps)
            {
                Assert.True(testCase.FirstRange.Overlaps(testCase.SecondRange),
                    $"{testCase.FirstRange} {testCase.SecondRange}");
                Assert.True(testCase.FirstRange.Overlaps(testCase.SecondRange),
                    $"{testCase.SecondRange} {testCase.FirstRange}");
            }
            else
            {
                Assert.False(testCase.FirstRange.Overlaps(testCase.SecondRange),
                    $"{testCase.FirstRange} {testCase.SecondRange}");
                Assert.False(testCase.FirstRange.Overlaps(testCase.SecondRange),
                    $"{testCase.SecondRange} {testCase.FirstRange}");
            }
        }

        public static RangeOverlapsTestCase Overlaps(UnbrokenSemVersionRange x, UnbrokenSemVersionRange y)
            => new RangeOverlapsTestCase(x, y, true);

        public static RangeOverlapsTestCase NoOverlap(UnbrokenSemVersionRange x, UnbrokenSemVersionRange y)
            => new RangeOverlapsTestCase(x, y, false);

        public static UnbrokenSemVersionRange Inclusive(string start, string end)
            => UnbrokenSemVersionRange.Inclusive(SemVersion.Parse(start, SemVersionStyles.Strict),
                SemVersion.Parse(end, SemVersionStyles.Strict));

        public static UnbrokenSemVersionRange InclusiveOfStart(string start, string end) =>
            UnbrokenSemVersionRange.InclusiveOfStart(SemVersion.Parse(start, SemVersionStyles.Strict),
                SemVersion.Parse(end, SemVersionStyles.Strict));

        public static UnbrokenSemVersionRange InclusiveOfEnd(string start, string end) =>
            UnbrokenSemVersionRange.InclusiveOfEnd(SemVersion.Parse(start, SemVersionStyles.Strict),
                SemVersion.Parse(end, SemVersionStyles.Strict));
    }
}
