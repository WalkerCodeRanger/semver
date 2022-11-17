using Semver.Ranges;
using Semver.Test.TestCases;
using Xunit;
using static Semver.Test.Builders.UnbrokenSemVersionRangeBuilder;

namespace Semver.Test.Ranges
{
    public class UnbrokenSemVersionRangeRelationsTests
    {
        public static readonly TheoryData<RangesRelatedTestCase> RangeContains = new TheoryData<RangesRelatedTestCase>()
        {
            Related(Empty, Empty),
            NotRelated(Empty, All),
            Related(All, Empty),
            Related(AllRelease, Empty),
            NotRelated(AllRelease, All),
            Related(All, AllRelease),
            Related(LessThan("4.5.6"), LessThan("1.2.3")),
            NotRelated(LessThan("1.2.3"), LessThan("4.5.6")),
        };

        public static readonly TheoryData<RangesRelatedTestCase> RangeOverlapsTestCases = new TheoryData<RangesRelatedTestCase>()
        {
            NotRelated(Empty, Empty),
            NotRelated(Empty, All),
            NotRelated(Empty, AllRelease),
            Related(All, All),
            Related(AllRelease, AllRelease),
            NotRelated(Inclusive("1.2.3", "4.5.6"), Inclusive("7.8.9", "8.2.3")),
            Related(Inclusive("1.2.3", "4.5.6"), Inclusive("4.5.6", "7.8.9")),
            NotRelated(InclusiveOfStart("1.2.3", "4.5.6"), InclusiveOfEnd("4.5.6", "7.8.9")),
            NotRelated(Inclusive("1.2.3", "4.5.6"), InclusiveOfEnd("4.5.6", "7.8.9")),
            NotRelated(InclusiveOfStart("1.2.3", "4.5.6"), Inclusive("4.5.6", "7.8.9")),
            Related(Inclusive("1.2.3", "4.5.6"), Inclusive("1.5.0", "3.5.4")),
        };

        [Theory]
        [MemberData(nameof(RangeContains))]
        public void ContainsIsCorrect(RangesRelatedTestCase testCase)
        {
            if (testCase.Related)
                Assert.True(testCase.X.Contains(testCase.Y), $"{testCase.X} contains {testCase.Y}");
            else
                Assert.False(testCase.X.Contains(testCase.Y), $"{testCase.X} contains {testCase.Y}");
        }

        [Theory]
        [MemberData(nameof(RangeOverlapsTestCases))]
        public void OverlapsIsCorrect(RangesRelatedTestCase testCase)
        {
            if (testCase.Related)
            {
                Assert.True(testCase.X.Overlaps(testCase.Y),
                    $"{testCase.X} {testCase.Y}");
                Assert.True(testCase.X.Overlaps(testCase.Y),
                    $"{testCase.Y} {testCase.X}");
            }
            else
            {
                Assert.False(testCase.X.Overlaps(testCase.Y),
                    $"{testCase.X} {testCase.Y}");
                Assert.False(testCase.X.Overlaps(testCase.Y),
                    $"{testCase.Y} {testCase.X}");
            }
        }

        public static RangesRelatedTestCase Related(UnbrokenSemVersionRange x, UnbrokenSemVersionRange y)
            => new RangesRelatedTestCase(x, y, true);

        public static RangesRelatedTestCase NotRelated(UnbrokenSemVersionRange x, UnbrokenSemVersionRange y)
            => new RangesRelatedTestCase(x, y, false);
    }
}
