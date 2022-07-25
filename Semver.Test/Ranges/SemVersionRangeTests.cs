using System.Diagnostics.CodeAnalysis;
using Semver.Ranges;
using Xunit;

namespace Semver.Test.Ranges
{
    public class SemVersionRangeTests
    {
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
    }
}
