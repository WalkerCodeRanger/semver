using System;
using System.Linq;
using Xunit;

namespace Semver.Test
{
    /// <summary>
    /// Tests for the <see cref="SemVersion.With"/> and <see cref="SemVersion.WithParsedFrom"/>
    /// methods. Each field change is tested independently to avoid combinatorial explosion.
    /// </summary>
    public class SemVersionWithTests
    {
        public static readonly SemVersion Version = SemVersion.ParsedFrom(1, 2, 3, "pre", "metadata");

        #region With(...)
        [Fact]
        public void WithMajor()
        {
            var v = Version.With(major: 42);

            Assert.Equal(SemVersion.ParsedFrom(42, 2, 3, "pre", "metadata"), v);
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

            Assert.Equal(SemVersion.ParsedFrom(1, 42, 3, "pre", "metadata"), v);
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

            Assert.Equal(SemVersion.ParsedFrom(1, 2, 42, "pre", "metadata"), v);
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
            var v = Version.With(prerelease: new[]
            {
                new PrereleaseIdentifier("bar"),
                new PrereleaseIdentifier("baz"),
                new PrereleaseIdentifier(100),
                new PrereleaseIdentifier("123abc"),
            });

            Assert.Equal(SemVersion.ParsedFrom(1, 2, 3, "bar.baz.100.123abc", "metadata"), v);
        }

        [Fact]
        public void WithPrereleaseEmpty()
        {
            var v = Version.With(prerelease: Enumerable.Empty<PrereleaseIdentifier>());

            Assert.Equal(SemVersion.ParsedFrom(1, 2, 3, "", "metadata"), v);
        }

        [Fact]
        public void WithPrereleaseDefaultIdentifier()
        {
            var ex = Assert.Throws<ArgumentException>(()
                => Version.With(prerelease: new PrereleaseIdentifier[] { default }));
            Assert.StartsWith("Prerelease identifier cannot be default/null.", ex.Message);
            Assert.Equal("prerelease", ex.ParamName);
        }

        [Fact]
        public void WithMetadata()
        {
            var v = Version.With(metadata: new[]
            {
                new MetadataIdentifier("bar"),
                new MetadataIdentifier("baz"),
                new MetadataIdentifier("100"),
                new MetadataIdentifier("123abc"),
            });

            Assert.Equal(SemVersion.ParsedFrom(1, 2, 3, "pre", "bar.baz.100.123abc"), v);
        }

        [Fact]
        public void WithMetadataEmpty()
        {
            var v = Version.With(metadata: Enumerable.Empty<MetadataIdentifier>());

            Assert.Equal(SemVersion.ParsedFrom(1, 2, 3, "pre"), v);
        }

        [Fact]
        public void WithMetadataDefaultIdentifier()
        {
            var ex = Assert.Throws<ArgumentException>(()
                => Version.With(metadata: new MetadataIdentifier[] { default }));
            Assert.StartsWith("Metadata identifier cannot be default/null.", ex.Message);
            Assert.Equal("metadata", ex.ParamName);
        }
        #endregion

        #region WithParsedFrom(...)
        [Fact]
        public void WithParsedFromMajor()
        {
            var v = Version.WithParsedFrom(major: 42);

            Assert.Equal(SemVersion.ParsedFrom(42, 2, 3, "pre", "metadata"), v);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public void WithParsedFromMajorInvalid(int majorVersion)
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => Version.WithParsedFrom(major: majorVersion));
            Assert.StartsWith("Major version must be greater than or equal to zero.", ex.Message);
            Assert.Equal("major", ex.ParamName);
        }

        [Fact]
        public void WithParsedFromMinor()
        {
            var v = Version.WithParsedFrom(minor: 42);

            Assert.Equal(SemVersion.ParsedFrom(1, 42, 3, "pre", "metadata"), v);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public void WithParsedFromMinorInvalid(int minorVersion)
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => Version.WithParsedFrom(minor: minorVersion));
            Assert.StartsWith("Minor version must be greater than or equal to zero.", ex.Message);
            Assert.Equal("minor", ex.ParamName);
        }

        [Fact]
        public void WithParsedFromPatch()
        {
            var v = Version.WithParsedFrom(patch: 42);

            Assert.Equal(SemVersion.ParsedFrom(1, 2, 42, "pre", "metadata"), v);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public void WithParsedFromPatchInvalid(int patchVersion)
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => Version.WithParsedFrom(patch: patchVersion));
            Assert.StartsWith("Patch version must be greater than or equal to zero.", ex.Message);
            Assert.Equal("patch", ex.ParamName);
        }

        [Fact]
        public void WithParsedFromPrerelease()
        {
            var v = Version.WithParsedFrom(prerelease: "bar.baz.100.123abc");

            Assert.Equal(SemVersion.ParsedFrom(1, 2, 3, "bar.baz.100.123abc", "metadata"), v);
        }

        [Fact]
        public void WithParsedFromPrereleaseEmptyString()
        {
            var v = Version.WithParsedFrom(prerelease: "");

            Assert.Equal(SemVersion.ParsedFrom(1, 2, 3, "", "metadata"), v);
        }

        [Fact]
        public void WithParsedFromPrereleaseEmptyIdentifier()
        {
            var ex = Assert.Throws<ArgumentException>(() => Version.WithParsedFrom(prerelease: "bar."));
            Assert.StartsWith("Prerelease identifier cannot be empty.", ex.Message);
            Assert.Equal("prerelease", ex.ParamName);
        }

        [Fact]
        public void WithParsedFromPrereleaseLeadingZeros()
        {
            var ex = Assert.Throws<ArgumentException>(()
                => Version.WithParsedFrom(prerelease: "bar.0123"));
            Assert.StartsWith("Leading zeros are not allowed on numeric prerelease identifiers '0123'.", ex.Message);
            Assert.Equal("prerelease", ex.ParamName);
        }

        [Fact]
        public void WithParsedFromPrereleaseLeadingZerosAllowed()
        {
            var v = Version.WithParsedFrom(prerelease: "bar.0123", allowLeadingZeros: true);

            Assert.Equal(SemVersion.ParsedFrom(1, 2, 3, "bar.123", "metadata"), v);
        }

        [Fact]
        public void WithParsedFromPrereleaseLargeNumber()
        {
            var v = Version.WithParsedFrom(prerelease: "bar.99999999999999999");

            Assert.Equal(SemVersion.ParsedFrom(1, 2, 3, "bar.99999999999999999", "metadata"), v);
        }

        [Fact]
        public void WithParsedFromPrereleaseInvalidCharacter()
        {
            var ex = Assert.Throws<ArgumentException>(()
                => Version.WithParsedFrom(prerelease: "bar.abc@123"));
            Assert.StartsWith("A prerelease identifier can contain only ASCII alphanumeric characters and hyphens 'abc@123'.", ex.Message);
            Assert.Equal("prerelease", ex.ParamName);
        }

        [Fact]
        public void WithParsedFromMetadata()
        {
            var v = Version.WithParsedFrom(metadata: "bar.baz.100.123abc");

            Assert.Equal(SemVersion.ParsedFrom(1, 2, 3, "pre", "bar.baz.100.123abc"), v);
        }

        [Fact]
        public void WithParsedFromMetadataEmptyString()
        {
            var v = Version.WithParsedFrom(metadata: "");

            Assert.Equal(SemVersion.ParsedFrom(1, 2, 3, "pre"), v);
        }

        [Fact]
        public void WithParsedFromMetadataEmptyIdentifier()
        {
            var ex = Assert.Throws<ArgumentException>(()
                => Version.WithParsedFrom(metadata: "bar."));
            Assert.StartsWith("Metadata identifier cannot be empty.", ex.Message);
            Assert.Equal("metadata", ex.ParamName);
        }

        [Fact]
        public void WithParsedFromMetadataLeadingZeros()
        {
            var v = Version.WithParsedFrom(metadata: "bar.0123");

            Assert.Equal(SemVersion.ParsedFrom(1, 2, 3, "pre", "bar.0123"), v);
        }

        [Fact]
        public void WithParsedFromMetadataTooLarge()
        {
            var v = Version.WithParsedFrom(metadata: "bar.99999999999999999");

            Assert.Equal(SemVersion.ParsedFrom(1, 2, 3, "pre", "bar.99999999999999999"), v);
        }

        [Fact]
        public void WithParsedFromMetadataInvalidCharacter()
        {
            var ex = Assert.Throws<ArgumentException>(()
                => Version.WithParsedFrom(metadata: "bar.abc@123"));
            Assert.StartsWith("A metadata identifier can contain only ASCII alphanumeric characters and hyphens 'abc@123'.", ex.Message);
            Assert.Equal("metadata", ex.ParamName);
        }
        #endregion
    }
}
