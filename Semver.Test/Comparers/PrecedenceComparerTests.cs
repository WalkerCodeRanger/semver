using System;
using System.Collections.Generic;
using Semver.Comparers;
using Xunit;

namespace Semver.Test.Comparers
{
    public class PrecedenceComparerTests
    {
        #region Equals
        [Fact]
        public void EqualsIdenticalTest()
        {
            foreach (var v in ComparerTestData.VersionsInSortOrder)
            {
                // Construct an identical version, but different instance
#pragma warning disable CS0618 // Type or member is obsolete
                var identical = new SemVersion(v.Major, v.Minor, v.Patch, v.Prerelease, v.Metadata);
#pragma warning restore CS0618 // Type or member is obsolete
                Assert.True(PrecedenceComparer.Instance.Equals(v, identical), v.ToString());
            }
        }

        [Fact]
        public void EqualsSameTest()
        {
            foreach (var v in ComparerTestData.VersionsInSortOrder)
                Assert.True(PrecedenceComparer.Instance.Equals(v, v), v.ToString());
        }

        [Fact]
        public void EqualsDifferentTest()
        {
            foreach (var (v1, v2) in ComparerTestData.VersionPairs)
            {
                var expected = EqualPrecedence(v1, v2);
                var actual = PrecedenceComparer.Instance.Equals(v1, v2);
                if (expected)
                    Assert.True(actual, $"Equals({v1}, {v2})");
                else
                    Assert.False(actual, $"Equals({v1}, {v2})");
            }
        }

        [Fact]
        public void EqualsNullTest()
        {
            Assert.True(PrecedenceComparer.Instance.Equals(null, null));
            foreach (var v in ComparerTestData.VersionsInSortOrder)
            {
                Assert.False(PrecedenceComparer.Instance.Equals(v, null), $"Equals({v}, null)");
                Assert.False(PrecedenceComparer.Instance.Equals(null, v), $"Equals(null, {v})");
            }
        }
        #endregion

        #region GetHashCode
        [Fact]
        public void GetHashCodeIdenticalTest()
        {
            foreach (var v in ComparerTestData.VersionsInSortOrder)
            {
                // Construct an identical version, but different instance
#pragma warning disable CS0618 // Type or member is obsolete
                var identical = new SemVersion(v.Major, v.Minor, v.Patch, v.Prerelease, v.Metadata);
#pragma warning restore CS0618 // Type or member is obsolete
                Assert.True(PrecedenceComparer.Instance.GetHashCode(v) == PrecedenceComparer.Instance.GetHashCode(identical), v.ToString());
            }
        }

        [Fact]
        public void GetHashCodeDifferentTest()
        {
            foreach (var (v1, v2) in ComparerTestData.VersionPairs)
            {
                var expected = EqualPrecedence(v1, v2);
                var actual = PrecedenceComparer.Instance.GetHashCode(v1) == PrecedenceComparer.Instance.GetHashCode(v2);
                if (expected)
                    Assert.True(actual, $"GetHashCode({v1}) == GetHashCode({v2})");
                else
                    Assert.False(actual, $"GetHashCode({v1}) == GetHashCode({v2})");
            }
        }
        #endregion

        #region CompareTo
        [Fact]
        public void CompareIdenticalTest()
        {
            foreach (var v in ComparerTestData.VersionsInSortOrder)
            {
                // Construct an identical version, but different instance
#pragma warning disable CS0618 // Type or member is obsolete
                var identical = new SemVersion(v.Major, v.Minor, v.Patch, v.Prerelease, v.Metadata);
#pragma warning restore CS0618 // Type or member is obsolete
                Assert.True(PrecedenceComparer.Instance.Compare(v, identical) == 0, v.ToString());
            }
        }

        [Fact]
        public void CompareSameTest()
        {
            foreach (var v in ComparerTestData.VersionsInSortOrder)
                Assert.True(PrecedenceComparer.Instance.Compare(v, v) == 0, v.ToString());
        }

        [Fact]
        public void CompareGreaterTest()
        {
            foreach (var (v1, v2) in ComparerTestData.VersionPairs)
            {
                var equal = EqualPrecedence(v1, v2);
                if (equal)
                    Assert.True(PrecedenceComparer.Instance.Compare(v1, v2) == 0, $"Compare({v1}, {v2}) == 0");
                else
                    Assert.True(PrecedenceComparer.Instance.Compare(v1, v2) == -1, $"Compare({v1}, {v2}) == -1");
            }
        }

        [Fact]
        public void CompareLesserTest()
        {
            foreach (var (v1, v2) in ComparerTestData.VersionPairs)
            {
                var equal = EqualPrecedence(v1, v2);
                if (equal)
                    Assert.True(PrecedenceComparer.Instance.Compare(v2, v1) == 0, $"Compare({v2}, {v1}) == 0");
                else
                    Assert.True(PrecedenceComparer.Instance.Compare(v2, v1) == 1, $"Compare({v2}, {v1}) == 1");
            }
        }

        [Fact]
        public void CompareObjectsTest()
        {
            var ex = Assert.Throws<ArgumentException>(()
                => PrecedenceComparer.Instance.Compare(new object(), new object()));

            Assert.Equal("Type of argument is not compatible with the generic comparer.", ex.Message);
        }

        [Fact]
        public void CompareNullTest()
        {
            foreach (var v in ComparerTestData.VersionsInSortOrder)
            {
                Assert.True(PrecedenceComparer.Instance.Compare(v, null) == 1, $"Compare({v}, null) == 1");
                Assert.True(PrecedenceComparer.Instance.Compare(null, v) == -1, $"Compare(null, {v}) == -1");
            }

            Assert.True(PrecedenceComparer.Instance.Compare(null, null) == 0, "Compare(null, null) == 0");
        }
        #endregion

        private static bool EqualPrecedence(SemVersion v1, SemVersion v2)
        {
            return v1.Major == v2.Major
                   && v1.Minor == v2.Minor
                   && v1.Patch == v2.Patch
                   && EqualPrecedence(v1.PrereleaseIdentifiers, v2.PrereleaseIdentifiers);
        }

        private static bool EqualPrecedence(
            IReadOnlyList<PrereleaseIdentifier> xIdentifiers,
            IReadOnlyList<PrereleaseIdentifier> yIdentifiers)
        {
            if (xIdentifiers.Count != yIdentifiers.Count) return false;

            for (int i = 0; i < xIdentifiers.Count; i++)
                if (xIdentifiers[i] != yIdentifiers[i])
                    return false;

            return true;
        }
    }
}
