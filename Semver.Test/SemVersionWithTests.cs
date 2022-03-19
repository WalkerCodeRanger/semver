using System;
using Xunit;

namespace Semver.Test
{
    /// <summary>
    /// Tests for the <see cref="SemVersion.WithParsedFrom"/> method. Each field change is
    /// tested independently to avoid combinatorial explosion.
    /// </summary>
    public class SemVersionWithTests
    {
        public static readonly SemVersion Version = new SemVersion(1, 2, 3, "pre", "metadata");

        #region WithParsedFrom(...)
        [Fact]
        public void WithMajor()
        {
            var v = Version.WithParsedFrom(major: 42);

            Assert.Equal(new SemVersion(42, 2, 3, "pre", "metadata"), v);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public void WithMajorInvalid(int majorVersion)
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => Version.WithParsedFrom(major: majorVersion));
            Assert.StartsWith("Major version must be greater than or equal to zero.", ex.Message);
            Assert.Equal("major", ex.ParamName);
        }

        [Fact]
        public void WithMinor()
        {
            var v = Version.WithParsedFrom(minor: 42);

            Assert.Equal(new SemVersion(1, 42, 3, "pre", "metadata"), v);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public void WithMinorInvalid(int minorVersion)
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => Version.WithParsedFrom(minor: minorVersion));
            Assert.StartsWith("Minor version must be greater than or equal to zero.", ex.Message);
            Assert.Equal("minor", ex.ParamName);
        }

        [Fact]
        public void WithPatch()
        {
            var v = Version.WithParsedFrom(patch: 42);

            Assert.Equal(new SemVersion(1, 2, 42, "pre", "metadata"), v);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public void WithPatchInvalid(int patchVersion)
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => Version.WithParsedFrom(patch: patchVersion));
            Assert.StartsWith("Patch version must be greater than or equal to zero.", ex.Message);
            Assert.Equal("patch", ex.ParamName);
        }

        [Fact]
        public void WithPrerelease()
        {
            var v = Version.WithParsedFrom(prerelease: "bar.baz.100.123abc");

            Assert.Equal(new SemVersion(1, 2, 3, "bar.baz.100.123abc", "metadata"), v);
        }

        [Fact]
        public void WithPrereleaseEmptyString()
        {
            var v = Version.WithParsedFrom(prerelease: "");

            Assert.Equal(new SemVersion(1, 2, 3, "", "metadata"), v);
        }

        [Fact]
        public void WithPrereleaseEmptyIdentifier()
        {
            var ex = Assert.Throws<ArgumentException>(() => Version.WithParsedFrom(prerelease: "bar."));
            Assert.StartsWith("Prerelease identifier cannot be empty.", ex.Message);
            Assert.Equal("prerelease", ex.ParamName);
        }

        [Fact]
        public void WithPrereleaseLeadingZeros()
        {
            var ex = Assert.Throws<ArgumentException>(()
                => Version.WithParsedFrom(prerelease: "bar.0123"));
            Assert.StartsWith("Leading zeros are not allowed on numeric prerelease identifiers '0123'.", ex.Message);
            Assert.Equal("prerelease", ex.ParamName);
        }

        [Fact]
        public void WithPrereleaseLeadingZerosAllowed()
        {
            var v = Version.WithParsedFrom(prerelease: "bar.0123", allowLeadingZeros: true);

            Assert.Equal(new SemVersion(1, 2, 3, "bar.123", "metadata"), v);
        }

        [Fact]
        public void WithPrereleaseTooLarge()
        {
            var ex = Assert.Throws<OverflowException>(()
               => Version.WithParsedFrom(prerelease: "bar.99999999999999999"));
            Assert.StartsWith("Prerelease identifier '99999999999999999' was too large for Int32.", ex.Message);
        }

        [Fact]
        public void WithPrereleaseInvalidCharacter()
        {
            var ex = Assert.Throws<ArgumentException>(()
                => Version.WithParsedFrom(prerelease: "bar.abc@123"));
            Assert.StartsWith("A prerelease identifier can contain only ASCII alphanumeric characters and hyphens 'abc@123'.", ex.Message);
            Assert.Equal("prerelease", ex.ParamName);
        }

        [Fact]
        public void WithMetadata()
        {
            var v = Version.WithParsedFrom(metadata: "bar.baz.100.123abc");

            Assert.Equal(new SemVersion(1, 2, 3, "pre", "bar.baz.100.123abc"), v);
        }

        [Fact]
        public void WithMetadataEmptyString()
        {
            var v = Version.WithParsedFrom(metadata: "");

            Assert.Equal(new SemVersion(1, 2, 3, "pre"), v);
        }

        [Fact]
        public void WithMetadataEmptyIdentifier()
        {
            var ex = Assert.Throws<ArgumentException>(()
                => Version.WithParsedFrom(metadata: "bar."));
            Assert.StartsWith("Metadata identifier cannot be empty.", ex.Message);
            Assert.Equal("metadata", ex.ParamName);
        }

        [Fact]
        public void WithMetadataLeadingZeros()
        {
            var v = Version.WithParsedFrom(metadata: "bar.0123");

            Assert.Equal(new SemVersion(1, 2, 3, "pre", "bar.0123"), v);
        }

        [Fact]
        public void WithMetadataTooLarge()
        {
            var v = Version.WithParsedFrom(metadata: "bar.99999999999999999");

            Assert.Equal(new SemVersion(1, 2, 3, "pre", "bar.99999999999999999"), v);
        }

        [Fact]
        public void WithMetadataInvalidCharacter()
        {
            var ex = Assert.Throws<ArgumentException>(()
                => Version.WithParsedFrom(metadata: "bar.abc@123"));
            Assert.StartsWith("A metadata identifier can contain only ASCII alphanumeric characters and hyphens 'abc@123'.", ex.Message);
            Assert.Equal("metadata", ex.ParamName);
        }
        #endregion
    }
}
