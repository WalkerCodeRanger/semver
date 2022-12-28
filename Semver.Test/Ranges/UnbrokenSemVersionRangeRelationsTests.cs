using Semver.Ranges;
using Semver.Test.TestCases;
using Xunit;
using static Semver.Test.Builders.UnbrokenSemVersionRangeBuilder;

namespace Semver.Test.Ranges
{
    public class UnbrokenSemVersionRangeRelationsTests
    {
        public static readonly TheoryData<RangesRelatedTestCase> RangeContains = new TheoryData<RangesRelatedTestCase>
        {
            Related(Empty, Empty),
            NotRelated(Empty, All),
            Related(All, Empty),
            Related(AllRelease, Empty),
            NotRelated(AllRelease, All),
            Related(All, AllRelease),
            Related(LessThan("4.5.6"), LessThan("1.2.3")),
            NotRelated(LessThan("1.2.3"), LessThan("4.5.6")),
            Related(InclusiveOfStart("1.0.0", "2.0.0-0"), InclusiveOfStart("1.2.0", "1.3.0-0")),
            Related(InclusiveOfStart("1.0.0-0", "1.0.0-rc"), InclusiveOfStart("1.0.0-alpha", "1.0.0-beta")),
            Related(InclusiveOfStart("1.0.0-0", "1.0.0-rc"), InclusiveOfStart("1.0.0-alpha", "1.0.0-beta", true)),
            NotRelated(Inclusive("1.0.0", "2.0.0"), Inclusive("1.0.0-rc", "2.0.0")),
            NotRelated(Inclusive("1.0.0-rc", "2.0.0"), Inclusive("1.0.1-rc", "2.0.0")),
            Related(Inclusive("1.0.0-alpha", "2.0.0"), Inclusive("1.0.0-rc", "2.0.0")),
            NotRelated(Inclusive("1.0.0", "2.0.0"), Inclusive("1.0.0", "2.0.0-rc")),
            NotRelated(Inclusive("1.0.0", "2.0.0-x"), Inclusive("1.0.0", "1.10.0-rc")),
            Related(Inclusive("1.0.0", "2.0.0-beta"), Inclusive("1.0.0", "2.0.0-alpha")),
        };

        public static readonly TheoryData<RangesRelatedTestCase> RangeOverlapsTestCases = new TheoryData<RangesRelatedTestCase>
        {
            NotRelated(Empty, Empty),
            NotRelated(Empty, All),
            NotRelated(Empty, AllRelease),
            Related(All, All),
            Related(AllRelease, AllRelease),
            Related(All, AllRelease),
            NotRelated(Inclusive("1.2.3", "4.5.6"), Inclusive("7.8.9", "8.2.3")),
            Related(Inclusive("1.2.3", "4.5.6"), Inclusive("4.5.6", "7.8.9")),
            NotRelated(InclusiveOfStart("1.2.3", "4.5.6"), InclusiveOfEnd("4.5.6", "7.8.9")),
            NotRelated(Inclusive("1.2.3", "4.5.6"), InclusiveOfEnd("4.5.6", "7.8.9")),
            NotRelated(InclusiveOfStart("1.2.3", "4.5.6"), Inclusive("4.5.6", "7.8.9")),
            Related(Inclusive("1.2.3", "4.5.6"), Inclusive("1.5.0", "3.5.4")),
        };

        /// <summary>
        /// This must repeat a number of the overlaps tests cases because abutting changes the outcome.
        /// </summary>
        public static readonly TheoryData<RangesRelatedTestCase> RangeOverlapsOrAbutsTestCases = new TheoryData<RangesRelatedTestCase>
        {
            NotRelated(Empty, Empty),
            NotRelated(Empty, All),
            NotRelated(Empty, AllRelease),
            Related(All, All),
            Related(AllRelease, AllRelease),
            Related(All, AllRelease),
            NotRelated(Inclusive("1.2.3", "4.5.6"), Inclusive("7.8.9", "8.2.3")),
            Related(Inclusive("1.2.3", "4.5.6"), Inclusive("4.5.6", "7.8.9")),
            NotRelated(InclusiveOfStart("1.2.3", "4.5.6"), InclusiveOfEnd("4.5.6", "7.8.9")),
            Related(Inclusive("1.2.3", "4.5.6"), InclusiveOfEnd("4.5.6", "7.8.9")),
            Related(InclusiveOfStart("1.2.3", "4.5.6"), Inclusive("4.5.6", "7.8.9")),
            Related(Inclusive("1.2.3", "4.5.6"), Inclusive("1.5.0", "3.5.4")),
            Related(InclusiveOfStart("1.0.0", "2.0.0-0"), InclusiveOfStart("2.0.0", "3.0.0-0")),
            NotRelated(InclusiveOfStart("1.0.0", "2.0.0-rc"), InclusiveOfStart("2.0.0", "3.0.0-0")),
        };

        public static readonly TheoryData<RangesUnionTestCase> RangeTryUnionTestCases = new TheoryData<RangesUnionTestCase>
        {
            Union(InclusiveOfStart("1.0.0", "2.0.0-0"), InclusiveOfStart("1.2.0", "1.3.0-0"),
                InclusiveOfStart("1.0.0", "2.0.0-0")),
            NoUnion(Inclusive("1.0.0","2.0.0",true), Inclusive("1.5.0","3.0.0")),
            NoUnion(Inclusive("1.0.0", "1.5.0"), Inclusive("2.0.0", "2.5.0")),
            NoUnion(Inclusive("1.0.0", "2.0.0"), Inclusive("1.5.0-rc", "3.0.0")),
            NoUnion(Inclusive("1.0.0", "2.0.0"), Inclusive("0.5.0", "1.5.0-rc")),
            Union(Inclusive("1.0.0","2.5.0"), Inclusive("2.5.0", "3.0.0"),
                Inclusive("1.0.0", "3.0.0")),
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
                Assert.True(testCase.Y.Overlaps(testCase.X),
                    $"{testCase.Y} {testCase.X}");
            }
            else
            {
                Assert.False(testCase.X.Overlaps(testCase.Y),
                    $"{testCase.X} {testCase.Y}");
                Assert.False(testCase.Y.Overlaps(testCase.X),
                    $"{testCase.Y} {testCase.X}");
            }
        }

        [Theory]
        [MemberData(nameof(RangeOverlapsOrAbutsTestCases))]
        public void OverlapsOrAbutsIsCorrect(RangesRelatedTestCase testCase)
        {
            if (testCase.Related)
            {
                Assert.True(testCase.X.OverlapsOrAbuts(testCase.Y), $"{testCase.X} {testCase.Y}");
                Assert.True(testCase.Y.OverlapsOrAbuts(testCase.X), $"{testCase.Y} {testCase.X}");
            }
            else
            {
                Assert.False(testCase.X.OverlapsOrAbuts(testCase.Y), $"{testCase.X} {testCase.Y}");
                Assert.False(testCase.Y.OverlapsOrAbuts(testCase.X), $"{testCase.Y} {testCase.X}");
            }
        }

        [Theory]
        [MemberData(nameof(RangeTryUnionTestCases))]
        public void TryUnionIsCorrect(RangesUnionTestCase testCase)
        {
            if (testCase.Expected is null)
            {
                Assert.False(testCase.X.TryUnion(testCase.Y, out _), $"{testCase.X} {testCase.Y}");
                Assert.False(testCase.Y.TryUnion(testCase.X, out _), $"{testCase.Y} {testCase.X}");
            }
            else
            {
                Assert.True(testCase.X.TryUnion(testCase.Y, out var union), $"{testCase.X} {testCase.Y}");
                Assert.Equal(testCase.Expected, union);
                Assert.True(testCase.Y.TryUnion(testCase.X, out _), $"{testCase.Y} {testCase.X}");
            }
        }

        public static RangesRelatedTestCase Related(UnbrokenSemVersionRange x, UnbrokenSemVersionRange y)
            => new RangesRelatedTestCase(x, y, true);

        public static RangesRelatedTestCase NotRelated(UnbrokenSemVersionRange x, UnbrokenSemVersionRange y)
            => new RangesRelatedTestCase(x, y, false);

        public static RangesUnionTestCase Union(
            UnbrokenSemVersionRange x,
            UnbrokenSemVersionRange y,
            UnbrokenSemVersionRange expected)
            => new RangesUnionTestCase(x, y, expected);

        public static RangesUnionTestCase NoUnion(
            UnbrokenSemVersionRange x,
            UnbrokenSemVersionRange y)
            => new RangesUnionTestCase(x, y);
    }
}
