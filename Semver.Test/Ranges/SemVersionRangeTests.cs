using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Semver.Ranges;
using Semver.Test.Builders;
using Xunit;

namespace Semver.Test.Ranges
{
    public class SemVersionRangeTests
    {
        private static readonly SemVersion VersionWithMetadata =
            SemVersion.Parse("1.2.3-foo+metadata", SemVersionStyles.Strict);
        private static readonly SemVersion FakeVersion = new SemVersion(1, 2, 3);

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
    }
}
