using System;
using Xunit;

namespace Semver.Test
{
    /// <summary>
    /// Tests of the "With..." methods of <see cref="SemVersion"/>.
    /// </summary>
    public class SemVersionWithTests
    {
        public static readonly SemVersion Version = new SemVersion(1, 2, 3, "pre", "metadata");

        [Fact]
        public void WithMajorVersion()
        {
            var v = Version.WithMajorVersion(42);

            Assert.Equal(new SemVersion(42, 2, 3, "pre", "metadata"), v);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public void WithMajorVersionInvalid(int majorVersion)
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => Version.WithMajorVersion(majorVersion));
            Assert.StartsWith("Major version must be greater than or equal to zero.", ex.Message);
            Assert.Equal("majorVersion", ex.ParamName);
        }

        [Fact]
        public void WithMinorVersion()
        {
            var v = Version.WithMinorVersion(42);

            Assert.Equal(new SemVersion(1, 42, 3, "pre", "metadata"), v);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public void WithMinorVersionInvalid(int minorVersion)
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => Version.WithMinorVersion(minorVersion));
            Assert.StartsWith("Minor version must be greater than or equal to zero.", ex.Message);
            Assert.Equal("minorVersion", ex.ParamName);
        }

        [Fact]
        public void WithPatchVersion()
        {
            var v = Version.WithPatchVersion(42);

            Assert.Equal(new SemVersion(1, 2, 42, "pre", "metadata"), v);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public void WithPatchVersionInvalid(int patchVersion)
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => Version.WithPatchVersion(patchVersion));
            Assert.StartsWith("Patch version must be greater than or equal to zero.", ex.Message);
            Assert.Equal("patchVersion", ex.ParamName);
        }
    }
}
