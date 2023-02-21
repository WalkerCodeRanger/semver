using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Semver.Test.Helpers;
using Xunit;

namespace Semver.Test
{
    public class SemVersionRangeTests
    {
        private static readonly SemVersion VersionWithMetadata
            = SemVersion.Parse("1.2.3-foo+metadata");
        private static readonly SemVersion FakeVersion = new SemVersion(1, 2, 3);
        private static readonly SemVersion FakeStartVersion = new SemVersion(1, 2, 3);
        private static readonly SemVersion FakeEndVersion = new SemVersion(2, 3, 4);
        private static readonly SemVersion FakePrereleaseVersion = SemVersion.ParsedFrom(1, 2, 3, "foo");

        [Fact]
        [SuppressMessage("Assertions", "xUnit2013:Do not use equality check to check for collection size.",
            Justification = "Need to validate count property.")]
        public void EmptyContainsNoRanges()
        {
            Assert.Equal(0, SemVersionRange.Empty.Count);
            Assert.Empty(SemVersionRange.Empty);
        }

        [Fact]
        public void AllReleaseIsUnbrokenRange()
        {
            Assert.Equal(new[] { UnbrokenSemVersionRange.AllRelease }, SemVersionRange.AllRelease);
        }

        [Fact]
        public void AllIsUnbrokenRange()
        {
            Assert.Equal(new[] { UnbrokenSemVersionRange.All }, SemVersionRange.All);
        }

        [Fact]
        public void EqualsNullVersion()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => SemVersionRange.Equals(null));
            Assert.StartsWith(ExceptionMessages.NotNull, ex.Message);
            Assert.Equal("version", ex.ParamName);
        }

        [Fact]
        public void EqualsMetadataVersion()
        {
            var ex = Assert.Throws<ArgumentException>(() => SemVersionRange.Equals(VersionWithMetadata));
            Assert.StartsWith(ExceptionMessages.NoMetadata, ex.Message);
            Assert.Equal("version", ex.ParamName);
        }

        [Fact]
        public void EqualsVersion()
        {
            var range = SemVersionRange.Equals(FakeVersion);

            Assert.Equal(new[] { UnbrokenSemVersionRange.Equals(FakeVersion) }, range);
        }

        [Fact]
        public void EqualsPrereleaseVersion()
        {
            var range = SemVersionRange.Equals(FakePrereleaseVersion);

            Assert.Equal(new[] { UnbrokenSemVersionRange.Equals(FakePrereleaseVersion) }, range);
        }

        [Fact]
        public void GreaterThanNullVersion()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => SemVersionRange.GreaterThan(null));
            Assert.StartsWith(ExceptionMessages.NotNull, ex.Message);
            Assert.Equal("version", ex.ParamName);
        }

        [Fact]
        public void GreaterThanMetadataVersion()
        {
            var ex = Assert.Throws<ArgumentException>(() => SemVersionRange.GreaterThan(VersionWithMetadata));
            Assert.StartsWith(ExceptionMessages.NoMetadata, ex.Message);
            Assert.Equal("version", ex.ParamName);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void GreaterThanVersion(bool includeAllPrerelease)
        {
            var range = SemVersionRange.GreaterThan(FakeVersion, includeAllPrerelease);

            Assert.Equal(new[] { UnbrokenSemVersionRange.GreaterThan(FakeVersion, includeAllPrerelease) }, range);
        }

        [Fact]
        public void GreaterThanMaxVersionEmpty()
        {
            var range = SemVersionRange.GreaterThan(SemVersion.Max);
            Assert.Same(SemVersionRange.Empty, range);
        }

        [Fact]
        public void AtLeastNullVersion()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => SemVersionRange.AtLeast(null));
            Assert.StartsWith(ExceptionMessages.NotNull, ex.Message);
            Assert.Equal("version", ex.ParamName);
        }

        [Fact]
        public void AtLeastMetadataVersion()
        {
            var ex = Assert.Throws<ArgumentException>(() => SemVersionRange.AtLeast(VersionWithMetadata));
            Assert.StartsWith(ExceptionMessages.NoMetadata, ex.Message);
            Assert.Equal("version", ex.ParamName);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void AtLeastVersion(bool includeAllPrerelease)
        {
            var range = SemVersionRange.AtLeast(FakeVersion, includeAllPrerelease);

            Assert.Equal(new[] { UnbrokenSemVersionRange.AtLeast(FakeVersion, includeAllPrerelease) }, range);
        }

        [Fact]
        public void LessThanNullVersion()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => SemVersionRange.LessThan(null));
            Assert.StartsWith(ExceptionMessages.NotNull, ex.Message);
            Assert.Equal("version", ex.ParamName);
        }

        [Fact]
        public void LessThanMetadataVersion()
        {
            var ex = Assert.Throws<ArgumentException>(() => SemVersionRange.LessThan(VersionWithMetadata));
            Assert.StartsWith(ExceptionMessages.NoMetadata, ex.Message);
            Assert.Equal("version", ex.ParamName);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void LessThanVersion(bool includeAllPrerelease)
        {
            var range = SemVersionRange.LessThan(FakeVersion, includeAllPrerelease);

            Assert.Equal(new[] { UnbrokenSemVersionRange.LessThan(FakeVersion, includeAllPrerelease) }, range);
        }

        [Fact]
        public void LessThanMinVersionEmpty()
        {
            var range = SemVersionRange.LessThan(SemVersion.Min, true);
            Assert.Same(SemVersionRange.Empty, range);
        }

        [Fact]
        public void LessThanMinReleaseVersionEmpty()
        {
            var range = SemVersionRange.LessThan(SemVersion.MinRelease);
            Assert.Same(SemVersionRange.Empty, range);
        }

        [Fact]
        public void LessThanMinReleaseVersionIncludingPrerelease()
        {
            var range = SemVersionRange.LessThan(SemVersion.MinRelease, true);

            Assert.Equal(new[] { UnbrokenSemVersionRange.LessThan(SemVersion.MinRelease, true) }, range);
        }

        [Fact]
        public void AtMostNullVersion()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => SemVersionRange.AtMost(null));
            Assert.StartsWith(ExceptionMessages.NotNull, ex.Message);
            Assert.Equal("version", ex.ParamName);
        }

        [Fact]
        public void AtMostMetadataVersion()
        {
            var ex = Assert.Throws<ArgumentException>(() => SemVersionRange.AtMost(VersionWithMetadata));
            Assert.StartsWith(ExceptionMessages.NoMetadata, ex.Message);
            Assert.Equal("version", ex.ParamName);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void AtMostVersion(bool includeAllPrerelease)
        {
            var range = SemVersionRange.AtMost(FakeVersion, includeAllPrerelease);

            Assert.Equal(new[] { UnbrokenSemVersionRange.AtMost(FakeVersion, includeAllPrerelease) }, range);
        }

        [Fact]
        public void InclusiveNullStartVersion()
        {
            var ex = Assert.Throws<ArgumentNullException>(
                () => SemVersionRange.Inclusive(null, FakeVersion));
            Assert.StartsWith(ExceptionMessages.NotNull, ex.Message);
            Assert.Equal("start", ex.ParamName);
        }

        [Fact]
        public void InclusiveMetadataStartVersion()
        {
            var ex = Assert.Throws<ArgumentException>(
                () => SemVersionRange.Inclusive(VersionWithMetadata, FakeVersion));
            Assert.StartsWith(ExceptionMessages.NoMetadata, ex.Message);
            Assert.Equal("start", ex.ParamName);
        }

        [Fact]
        public void InclusiveNullEndVersion()
        {
            var ex = Assert.Throws<ArgumentNullException>(
                () => SemVersionRange.Inclusive(FakeVersion, null));
            Assert.StartsWith(ExceptionMessages.NotNull, ex.Message);
            Assert.Equal("end", ex.ParamName);
        }

        [Fact]
        public void InclusiveMetadataEndVersion()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                SemVersionRange.Inclusive(FakeVersion, VersionWithMetadata));
            Assert.StartsWith(ExceptionMessages.NoMetadata, ex.Message);
            Assert.Equal("end", ex.ParamName);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void InclusiveVersion(bool includeAllPrerelease)
        {
            var range = SemVersionRange.Inclusive(FakeStartVersion, FakeEndVersion, includeAllPrerelease);

            Assert.Equal(new[] { UnbrokenSemVersionRange.Inclusive(FakeStartVersion, FakeEndVersion, includeAllPrerelease) }, range);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void InclusiveEmpty(bool includeAllPrerelease)
        {
            var range = SemVersionRange.Inclusive(new SemVersion(1, 2, 3), new SemVersion(1, 2, 2),
                            includeAllPrerelease);
            Assert.Same(SemVersionRange.Empty, range);
        }

        [Fact]
        public void InclusiveOfStartNullStartVersion()
        {
            var ex = Assert.Throws<ArgumentNullException>(
                () => SemVersionRange.InclusiveOfStart(null, FakeVersion));
            Assert.StartsWith(ExceptionMessages.NotNull, ex.Message);
            Assert.Equal("start", ex.ParamName);
        }

        [Fact]
        public void InclusiveOfStartMetadataStartVersion()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                SemVersionRange.InclusiveOfStart(VersionWithMetadata, FakeVersion));
            Assert.StartsWith(ExceptionMessages.NoMetadata, ex.Message);
            Assert.Equal("start", ex.ParamName);
        }

        [Fact]
        public void InclusiveOfStartNullEndVersion()
        {
            var ex = Assert.Throws<ArgumentNullException>(
                () => SemVersionRange.InclusiveOfStart(FakeVersion, null));
            Assert.StartsWith(ExceptionMessages.NotNull, ex.Message);
            Assert.Equal("end", ex.ParamName);
        }

        [Fact]
        public void InclusiveOfStartMetadataEndVersion()
        {
            var ex = Assert.Throws<ArgumentException>(
                () => SemVersionRange.InclusiveOfStart(FakeVersion, VersionWithMetadata));
            Assert.StartsWith(ExceptionMessages.NoMetadata, ex.Message);
            Assert.Equal("end", ex.ParamName);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void InclusiveOfStartVersion(bool includeAllPrerelease)
        {
            var range = SemVersionRange.InclusiveOfStart(FakeStartVersion, FakeEndVersion, includeAllPrerelease);

            Assert.Equal(
                new[] { UnbrokenSemVersionRange.InclusiveOfStart(FakeStartVersion, FakeEndVersion, includeAllPrerelease) },
                range);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void InclusiveOfStartEmpty(bool includeAllPrerelease)
        {
            var range = SemVersionRange.InclusiveOfStart(new SemVersion(1, 2, 3), new SemVersion(1, 2, 3),
                includeAllPrerelease);
            Assert.Same(SemVersionRange.Empty, range);
        }

        [Fact]
        public void InclusiveOfEndNullStartVersion()
        {
            var ex = Assert.Throws<ArgumentNullException>(
                () => SemVersionRange.InclusiveOfEnd(null, FakeVersion));
            Assert.StartsWith(ExceptionMessages.NotNull, ex.Message);
            Assert.Equal("start", ex.ParamName);
        }

        [Fact]
        public void InclusiveOfEndMetadataStartVersion()
        {
            var ex = Assert.Throws<ArgumentException>(
                () => SemVersionRange.InclusiveOfEnd(VersionWithMetadata, FakeVersion));
            Assert.StartsWith(ExceptionMessages.NoMetadata, ex.Message);
            Assert.Equal("start", ex.ParamName);
        }

        [Fact]
        public void InclusiveOfEndNullEndVersion()
        {
            var ex = Assert.Throws<ArgumentNullException>(
                () => SemVersionRange.InclusiveOfEnd(FakeVersion, null));
            Assert.StartsWith(ExceptionMessages.NotNull, ex.Message);
            Assert.Equal("end", ex.ParamName);
        }

        [Fact]
        public void InclusiveOfEndMetadataEndVersion()
        {
            var ex = Assert.Throws<ArgumentException>(
                () => SemVersionRange.InclusiveOfEnd(FakeVersion, VersionWithMetadata));
            Assert.StartsWith(ExceptionMessages.NoMetadata, ex.Message);
            Assert.Equal("end", ex.ParamName);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void InclusiveOfEndVersion(bool includeAllPrerelease)
        {
            var range = SemVersionRange.InclusiveOfEnd(FakeStartVersion, FakeEndVersion, includeAllPrerelease);

            Assert.Equal(new[] { UnbrokenSemVersionRange.InclusiveOfEnd(FakeStartVersion, FakeEndVersion, includeAllPrerelease) }, range);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void InclusiveOfEndEmpty(bool includeAllPrerelease)
        {
            var range = SemVersionRange.InclusiveOfEnd(new SemVersion(1, 2, 3), new SemVersion(1, 2, 3),
                            includeAllPrerelease);
            Assert.Same(SemVersionRange.Empty, range);
        }

        [Fact]
        public void ExclusiveNullStartVersion()
        {
            var ex = Assert.Throws<ArgumentNullException>(
                () => SemVersionRange.Exclusive(null, FakeVersion));
            Assert.StartsWith(ExceptionMessages.NotNull, ex.Message);
            Assert.Equal("start", ex.ParamName);
        }

        [Fact]
        public void ExclusiveMetadataStartVersion()
        {
            var ex = Assert.Throws<ArgumentException>(
                () => SemVersionRange.Exclusive(VersionWithMetadata, FakeVersion));
            Assert.StartsWith(ExceptionMessages.NoMetadata, ex.Message);
            Assert.Equal("start", ex.ParamName);
        }

        [Fact]
        public void ExclusiveNullEndVersion()
        {
            var ex = Assert.Throws<ArgumentNullException>(
                () => SemVersionRange.Exclusive(FakeVersion, null));
            Assert.StartsWith(ExceptionMessages.NotNull, ex.Message);
            Assert.Equal("end", ex.ParamName);
        }

        [Fact]
        public void ExclusiveMetadataEndVersion()
        {
            var ex = Assert.Throws<ArgumentException>(
                () => SemVersionRange.Exclusive(FakeVersion, VersionWithMetadata));
            Assert.StartsWith(ExceptionMessages.NoMetadata, ex.Message);
            Assert.Equal("end", ex.ParamName);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ExclusiveVersion(bool includeAllPrerelease)
        {
            var range = SemVersionRange.Exclusive(FakeStartVersion, FakeEndVersion, includeAllPrerelease);

            Assert.Equal(new[] { UnbrokenSemVersionRange.Exclusive(FakeStartVersion, FakeEndVersion, includeAllPrerelease) }, range);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ExclusiveEmpty(bool includeAllPrerelease)
        {
            var range = SemVersionRange.Exclusive(new SemVersion(1, 2, 3), new SemVersion(1, 2, 3),
                            includeAllPrerelease);
            Assert.Same(SemVersionRange.Empty, range);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ExclusiveAtMaxEmpty(bool includeAllPrerelease)
        {
            var range = SemVersionRange.Exclusive(SemVersion.Max, SemVersion.Max, includeAllPrerelease);
            Assert.Same(SemVersionRange.Empty, range);
        }

        [Fact]
        public void CreateEnumerableEmpty()
        {
            var range = SemVersionRange.Create(Enumerable.Empty<UnbrokenSemVersionRange>());

            Assert.Same(SemVersionRange.Empty, range);
        }

        [Fact]
        public void CreateNullEnumerable()
        {
            var ex = Assert.Throws<ArgumentNullException>(()
                => SemVersionRange.Create((IEnumerable<UnbrokenSemVersionRange>)null));

            Assert.StartsWith(ExceptionMessages.NotNull, ex.Message);
            Assert.Equal("ranges", ex.ParamName);
        }

        [Fact]
        public void CreateEnumerableOfEmptyUnbrokenRange()
        {
            var range = SemVersionRange.Create(new[] { UnbrokenSemVersionRange.Empty }.AsEnumerable());

            Assert.Same(SemVersionRange.Empty, range);
        }

        [Fact]
        public void CreateParamsEmpty()
        {
            var range = SemVersionRange.Create();

            Assert.Same(SemVersionRange.Empty, range);
        }

        [Fact]
        public void CreateNullArray()
        {
            var ex = Assert.Throws<ArgumentNullException>(() =>
                SemVersionRange.Create((UnbrokenSemVersionRange[])null));

            Assert.StartsWith(ExceptionMessages.NotNull, ex.Message);
            Assert.Equal("ranges", ex.ParamName);
        }

        [Fact]
        public void CreatePramsOfEmptyUnbrokenRange()
        {
            var range = SemVersionRange.Create(UnbrokenSemVersionRange.Empty);

            Assert.Same(SemVersionRange.Empty, range);
        }

        [Fact]
        public void Indexer()
        {
            var range = SemVersionRange.Create(UnbrokenSemVersionRange.All);

            Assert.Equal(UnbrokenSemVersionRange.All, range[0]);
        }

        [Fact]
        public void GetHashCodeAndEquality()
        {
            var range1 = SemVersionRange.Parse("1.*");
            var range2 = SemVersionRange.Parse("1.*");

            Assert.NotSame(range1, range2);
            Assert.Equal(range1.GetHashCode(), range2.GetHashCode());
            Assert.True(range1 == range2);
            Assert.False(range1 != range2);
        }

        [Fact]
        public void EqualsNull()
        {
            Assert.False(SemVersionRange.AllRelease.Equals(null));
        }

        [Fact]
        public void ToStringOfRangeWithSingleUnbrokenRange()
        {
            var range = SemVersionRange.Parse(">= 1.2.3");

            Assert.Equal(">=1.2.3", range.ToString());
        }

        [Fact]
        public void ToStringOfEmpty()
        {
            Assert.Equal("", SemVersionRange.Empty.ToString());
        }

        [Fact]
        public void ToStringOfRangeWithMultipleUnbrokenRanges()
        {
            var range = SemVersionRange.Parse(" >= 5.6.7 ||  < 1.2.3");

            Assert.Equal("<1.2.3 || >=5.6.7", range.ToString());
        }
    }
}
