using System;
using Xunit;

namespace Semver.Test
{
    /// <summary>
    /// Tests for the <see cref="SemVersion.With"/> method. Each field change is
    /// tested independently to avoid combinatorial explosion.
    /// </summary>
    public class SemVersionWithTests
    {
        public static readonly SemVersion Version = new SemVersion(1, 2, 3, "pre", "metadata");

        [Fact]
        public void WithMajor()
        {
            var v = Version.With(major: 42);

            Assert.Equal(new SemVersion(42, 2, 3, "pre", "metadata"), v);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public void WithMajorInvalid(int majorVersion)
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => Version.With(major: majorVersion));
            Assert.StartsWith("Major version must be greater than or equal to zero.", ex.Message);
            Assert.Equal("major", ex.ParamName);
        }

        [Fact]
        public void WithMinor()
        {
            var v = Version.With(minor: 42);

            Assert.Equal(new SemVersion(1, 42, 3, "pre", "metadata"), v);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public void WithMinorInvalid(int minorVersion)
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => Version.With(minor: minorVersion));
            Assert.StartsWith("Minor version must be greater than or equal to zero.", ex.Message);
            Assert.Equal("minor", ex.ParamName);
        }

        [Fact]
        public void WithPatch()
        {
            var v = Version.With(patch: 42);

            Assert.Equal(new SemVersion(1, 2, 42, "pre", "metadata"), v);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public void WithPatchInvalid(int patchVersion)
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => Version.With(patch: patchVersion));
            Assert.StartsWith("Patch version must be greater than or equal to zero.", ex.Message);
            Assert.Equal("patch", ex.ParamName);
        }

        [Fact]
        public void WithPrerelease()
        {
            var v = Version.With(prerelease: "bar.baz.100.123abc");

            Assert.Equal(new SemVersion(1, 2, 3, "bar.baz.100.123abc", "metadata"), v);
        }

        [Fact]
        public void WithPrereleaseEmptyString()
        {
            var v = Version.With(prerelease: "");

            Assert.Equal(new SemVersion(1, 2, 3, "", "metadata"), v);
        }

        [Fact]
        public void WithPrereleaseEmptyIdentifier()
        {
            var ex = Assert.Throws<ArgumentException>(() => Version.With(prerelease: "bar."));
            Assert.StartsWith("Prerelease identifier cannot be empty.", ex.Message);
            Assert.Equal("prerelease", ex.ParamName);
        }

        [Fact]
        public void WithPrereleaseLeadingZeros()
        {
            var ex = Assert.Throws<ArgumentException>(()
                => Version.With(prerelease: "bar.0123", allowLeadingZeros: false));
            Assert.StartsWith("Leading zeros are not allowed on numeric prerelease identifiers '0123'.", ex.Message);
            Assert.Equal("prerelease", ex.ParamName);
        }

        [Fact]
        public void WithPrereleaseLeadingZerosAllowed()
        {
            var v = Version.With(prerelease: "bar.0123", allowLeadingZeros: true);

            Assert.Equal(new SemVersion(1, 2, 3, "bar.123", "metadata"), v);
        }

        [Fact]
        public void WithPrereleaseTooLarge()
        {
            var ex = Assert.Throws<OverflowException>(()
               => Version.With(prerelease: "bar.99999999999999999", allowLeadingZeros: false));
            Assert.StartsWith("Prerelease identifier '99999999999999999' was too large for Int32.", ex.Message);
        }

        [Fact]
        public void WithPrereleaseInvalidCharacter()
        {
            var ex = Assert.Throws<ArgumentException>(()
                => Version.With(prerelease: "bar.abc@123", allowLeadingZeros: false));
            Assert.StartsWith("A prerelease identifier can contain only ASCII alphanumeric characters and hyphens 'abc@123'.", ex.Message);
            Assert.Equal("prerelease", ex.ParamName);
        }

        [Fact]
        public void WithMetadata()
        {
            var v = Version.With(metadata: "bar.baz.100.123abc");

            Assert.Equal(new SemVersion(1, 2, 3, "pre", "bar.baz.100.123abc"), v);
        }

        [Fact]
        public void WithMetadataEmptyString()
        {
            var v = Version.With(metadata: "");

            Assert.Equal(new SemVersion(1, 2, 3, "pre"), v);
        }

        [Fact]
        public void WithMetadataEmptyIdentifier()
        {
            var ex = Assert.Throws<ArgumentException>(()
                => Version.With(metadata: "bar."));
            Assert.StartsWith("Metadata identifier cannot be empty.", ex.Message);
            Assert.Equal("metadata", ex.ParamName);
        }

        [Fact]
        public void WithMetadataLeadingZeros()
        {
            var v = Version.With(metadata: "bar.0123");

            Assert.Equal(new SemVersion(1, 2, 3, "pre", "bar.0123"), v);
        }

        [Fact]
        public void WithMetadataTooLarge()
        {
            var v = Version.With(metadata: "bar.99999999999999999");

            Assert.Equal(new SemVersion(1, 2, 3, "pre", "bar.99999999999999999"), v);
        }

        [Fact]
        public void WithMetadataInvalidCharacter()
        {
            var ex = Assert.Throws<ArgumentException>(()
                => Version.With(metadata: "bar.abc@123"));
            Assert.StartsWith("A metadata identifier can contain only ASCII alphanumeric characters and hyphens 'abc@123'.", ex.Message);
            Assert.Equal("metadata", ex.ParamName);
        }
    }
}
