using System;
using Xunit;

namespace Semver.Test
{
    public class SemVersionComparisonTests
    {
        #region Equality

        [Theory]
        [InlineData("1.2+nightly", "1.2+nightly", true)]
        [InlineData("1.2-alpha+dev", "1.2-alpha+dev", true)]
        [InlineData("1.2-nightly+dev", "1.2.0-nightly", false)]
        [InlineData("1.2-nightly", "1.2.0-nightly2", false)]
        [InlineData("1.2.1", "1.2.0", false)]
        [InlineData("1.4.0", "1.2.0", false)]
        [InlineData("1.2.3-a", "1.2.3-A", false)]
        public void EqualsTest(string s1, string s2, bool equal)
        {
            var v1 = SemVersion.Parse(s1);
            var v2 = SemVersion.Parse(s2);

            var r = v1.Equals(v2);

            Assert.Equal(equal, r);
        }

        [Fact]
        public void EqualsSameTest()
        {
            var v1 = SemVersion.Parse("1.2-nightly");

            var r = v1.Equals(v1);
            Assert.True(r);
        }

        [Fact]
        public void EqualsNonSemVersionTest()
        {
            var v = new SemVersion(1);
            // TODO should throw argument exception
            var ex = Assert.Throws<InvalidCastException>(() => v.CompareTo(new object()));

            Assert.Equal("Unable to cast object of type 'System.Object' to type 'Semver.SemVersion'.", ex.Message);
        }

        [Theory]
        [InlineData("1.2.3", "1.2.3", true)]
        [InlineData(null, "1.2.3", false)]
        [InlineData("1.2.3", null, false)]
        [InlineData(null, null, true)]
        public void StaticEqualsTest(string s1, string s2, bool equal)
        {
            var v1 = s1 is null ? null : SemVersion.Parse(s1);
            var v2 = s2 is null ? null : SemVersion.Parse(s2);

            var r = SemVersion.Equals(v1, v2);

            Assert.Equal(equal, r);
        }

        #endregion

        #region Comparison
        [Theory]
        [InlineData("1.0.0-alpha+dev.123", "1.0.0-beta+dev.123", -1)]
        [InlineData("1.0.0", "1.0.1-alpha", -1)]
        [InlineData("0.0.1", "0.0.1+build.12", -1)]
        [InlineData("0.0.1+build.13", "0.0.1+build.12.2", 1)]
        [InlineData("0.0.1-13", "0.0.1-b", -1)]
        [InlineData("0.0.1+uiui", "0.0.1+12", 1)]
        [InlineData("0.0.1+bu", "0.0.1", 1)]
        [InlineData("0.1.1+bu", "0.2.1", -1)]
        [InlineData("0.1.1-gamma.12.87", "0.1.1-gamma.12.88", -1)]
        [InlineData("0.1.1-gamma.12.87", "0.1.1-gamma.12.87.1", -1)]
        [InlineData("0.1.1-gamma.12.87.99", "0.1.1-gamma.12.87.X", -1)]
        [InlineData("0.1.1-gamma.12.87", "0.1.1-gamma.12.87.X", -1)]
        // Examples from spec
        [InlineData("1.0.0", "2.0.0", -1)]
        [InlineData("2.0.0", "2.1.0", -1)]
        [InlineData("2.1.0", "2.1.1", -1)]
        [InlineData("1.0.0-alpha", "1.0.0", -1)]
        [InlineData("1.0.0-alpha", "1.0.0-alpha.1", -1)]
        [InlineData("1.0.0-alpha.1", "1.0.0-alpha.beta", -1)]
        [InlineData("1.0.0-alpha.beta", "1.0.0-beta", -1)]
        [InlineData("1.0.0-beta", "1.0.0-beta.2", -1)]
        [InlineData("1.0.0-beta.2", "1.0.0-beta.11", -1)]
        // TODO comparison distinguished by string order currently gives numbers outside -1,0,1 (issue #26)
        [InlineData("1.0.0-beta.11", "1.0.0-rc.1", -16)]
        [InlineData("1.0.0-rc.1", "1.0.0", -1)]
        public void CompareToTest(string s1, string s2, int expected)
        {
            var v1 = SemVersion.Parse(s1, true);
            var v2 = SemVersion.Parse(s2, true);

            var r1 = v1.CompareTo(v2);
            var r2 = v2.CompareTo(v1);

            Assert.Equal(expected, r1);
            Assert.Equal(-expected, r2);
        }

        [Fact]
        public void CompareToNullTest()
        {
            var v1 = SemVersion.Parse("0.0.1+bu");
            var r = v1.CompareTo(null);
            Assert.Equal(1, r);
        }

        [Fact]
        public void CompareToSemVersionAsObjectTest()
        {
            var v1 = new SemVersion(1);
            var c = v1.CompareTo((object)v1);

            Assert.Equal(0, c);
        }

        [Fact]
        public void CompareToNonSemVersionTest()
        {
            var v = new SemVersion(1);
            // TODO issue #39 should throw argument exception
            var ex = Assert.Throws<InvalidCastException>(() => v.CompareTo(new object()));

            Assert.Equal("Unable to cast object of type 'System.Object' to type 'Semver.SemVersion'.", ex.Message);
        }

        [Theory]
        [InlineData("1.0.0", "2.0.0", -1)]
        [InlineData("1.0.0", "1.0.0", 0)]
        [InlineData(null, "1.0.0", -1)]
        [InlineData("1.0.0", null, 1)]
        [InlineData(null, null, 0)]
        public void StaticCompareTest(string s1, string s2, int expected)
        {
            var v1 = s1 is null ? null : SemVersion.Parse(s1);
            var v2 = s2 is null ? null : SemVersion.Parse(s2);

            var r = SemVersion.Compare(v1, v2);
            Assert.Equal(expected, r);
        }
        #endregion

        #region Precedence
        [Theory]
        [InlineData("1.2+nightly", "1.2+nightly", true)]
        [InlineData("1.2-alpha+dev", "1.2-alpha+dev", true)]
        [InlineData("1.2-nightly+dev", "1.2.0-nightly", true)]
        [InlineData("1.2-nightly+45", "1.2.0-nightly+ab", true)]
        [InlineData("1.2-nightly", "1.2.0-nightly2", false)]
        [InlineData("1.2.1", "1.2.0", false)]
        [InlineData("1.4.0", "1.2.0", false)]
        public void PrecedenceMatchesTest(string s1, string s2, bool equal)
        {
            var v1 = SemVersion.Parse(s1);
            var v2 = SemVersion.Parse(s2);

            var r = v1.PrecedenceMatches(v2);

            Assert.Equal(equal, r);
        }


        [Theory]
        [InlineData("1.0.0-alpha+dev.123", "1.0.0-beta+dev.123", -1)]
        [InlineData("1.0.0", "1.0.1-alpha", -1)]
        [InlineData("0.0.1", "0.0.1+build.12", 0)]
        [InlineData("0.0.1+build.13", "0.0.1+build.12.2", 0)]
        [InlineData("0.0.1-13", "0.0.1-b", -1)]
        [InlineData("0.0.1+uiui", "0.0.1+12", 0)]
        [InlineData("0.0.1+bu", "0.0.1", 0)]
        [InlineData("0.1.1+bu", "0.2.1", -1)]
        [InlineData("0.1.1-gamma.12.87", "0.1.1-gamma.12.88", -1)]
        [InlineData("0.1.1-gamma.12.87", "0.1.1-gamma.12.87.1", -1)]
        [InlineData("0.1.1-gamma.12.87.99", "0.1.1-gamma.12.87.X", -1)]
        [InlineData("0.1.1-gamma.12.87", "0.1.1-gamma.12.87.X", -1)]
        // Examples from spec
        [InlineData("1.0.0", "2.0.0", -1)]
        [InlineData("2.0.0", "2.1.0", -1)]
        [InlineData("2.1.0", "2.1.1", -1)]
        [InlineData("1.0.0-alpha", "1.0.0", -1)]
        [InlineData("1.0.0-alpha", "1.0.0-alpha.1", -1)]
        [InlineData("1.0.0-alpha.1", "1.0.0-alpha.beta", -1)]
        [InlineData("1.0.0-alpha.beta", "1.0.0-beta", -1)]
        [InlineData("1.0.0-beta", "1.0.0-beta.2", -1)]
        [InlineData("1.0.0-beta.2", "1.0.0-beta.11", -1)]
        // TODO comparison distinguished by string order currently gives numbers outside -1,0,1 (issue #26)
        [InlineData("1.0.0-beta.11", "1.0.0-rc.1", -16)]
        [InlineData("1.0.0-rc.1", "1.0.0", -1)]
        public void CompareByPrecedenceTest(string s1, string s2, int expected)
        {
            var v1 = SemVersion.Parse(s1, true);
            var v2 = SemVersion.Parse(s2, true);

            var r1 = v1.CompareByPrecedence(v2);
            var r2 = v2.CompareByPrecedence(v1);

            Assert.Equal(expected, r1);
            Assert.Equal(-expected, r2);
        }

        [Fact]
        public void CompareByPrecedenceToNullTest()
        {
            var v1 = SemVersion.Parse("0.0.1+bu");
            var r = v1.CompareByPrecedence(null);
            Assert.Equal(1, r);
        }
        #endregion

        #region Operators

        [Fact]
        public void EqualsOperatorTest()
        {
            var v1 = new SemVersion(1);
            var v2 = new SemVersion(1);

            var r = v1 == v2;
            Assert.True(r);
        }

        [Fact]
        public void NotEqualOperatorTest()
        {
            var v1 = new SemVersion(1);
            var v2 = new SemVersion(2);

            var r = v1 != v2;
            Assert.True(r);
        }

        [Theory]
        [InlineData("1.0.0", "2.0.0")]
        [InlineData("1.0.0-alpha", "1.0.0-rc")]
        [InlineData("1.0.0-alpha", "1.0.0-ci.1")]
        [InlineData("1.0.0-A", "1.0.0-a")]
        public void ComparisonOperatorTest(string s1, string s2)
        {
            var v1 = SemVersion.Parse(s1);
            var v2 = SemVersion.Parse(s2);

            Assert.True(v1 < v2, "v1 < v2");
            Assert.True(v2 > v1, "v2 > v1");
        }

        [Theory]
        [InlineData("1.0.0", "2.0.0")]
        [InlineData("1.0.0-alpha", "1.0.0-rc")]
        [InlineData("1.0.0-alpha", "1.0.0-ci.1")]
        public void CompareOrEqualOperatorTest(string s1, string s2)
        {
            var v1 = SemVersion.Parse(s1);
            var v2 = SemVersion.Parse(s2);

            Assert.True(v1 <= v2, "v1 <= v2");
            Assert.True(v2 >= v1, "v2 >= v1");
        }

        [Theory]
        [InlineData("1.0.0", "1.0.0")]
        [InlineData("1.0.0-alpha", "1.0.0-alpha")]
        [InlineData("1.0.0-rc.1", "1.0.0-rc.1")]
        [InlineData("1.0.0-beta-34+0f45", "1.0.0-beta-34+0f45")]
        public void CompareOrEqualOperatorWhenEqualTest(string s1, string s2)
        {
            var v1 = SemVersion.Parse(s1);
            var v2 = SemVersion.Parse(s2);

            Assert.True(v1 <= v2, "v1 <= v2");
            Assert.True(v1 >= v2, "v1 >= v2");
        }

        #endregion
    }
}
