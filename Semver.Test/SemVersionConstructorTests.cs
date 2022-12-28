using System;
using System.Collections.Generic;
using System.Linq;
using Semver.Test.Helpers;
using Xunit;

namespace Semver.Test
{
    public class SemVersionConstructorTests
    {
        private const string InvalidMajorVersionMessage = "Major version must be greater than or equal to zero.";
        private const string InvalidMinorVersionMessage = "Minor version must be greater than or equal to zero.";
        private const string InvalidPatchVersionMessage = "Patch version must be greater than or equal to zero.";
        private const string PrereleaseIdentifierIsDefaultMessage = "Prerelease identifier cannot be default/null.";
        private const string MetadataIdentifierIsDefaultMessage = "Metadata identifier cannot be default/null.";

        /// <summary>
        /// This test shows that named arguments will resolve to a constructor with few parameters
        /// rather than one with more parameters that might be obsolete.
        /// </summary>
        [Fact]
        public void NamedArgumentsResolveToShortestConstructor()
        {
            var v = new SemVersion(major: 2, minor: 1, patch: 3);

            Assert.Equal(new SemVersion(2, 1, 3), v);
        }

        #region SemVersion(int major)
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        // TODO Negative versions should be invalid and throw argument exceptions (issue#41)
        [InlineData(-1)]
        public void ConstructWithMajorTest(int major)
        {
            var v = new SemVersion(major);

            Assert.Equal(major, v.Major);
            Assert.Equal(0, v.Minor);
            Assert.Equal(0, v.Patch);
            Assert.Equal("", v.Prerelease);
            Assert.Empty(v.PrereleaseIdentifiers);
            Assert.Equal("", v.Metadata);
            Assert.Empty(v.MetadataIdentifiers);
        }
        #endregion

        #region SemVersion(int major, int minor)
        [Theory]
        [InlineData(1, 2)]
        // TODO Negative versions should be invalid and throw argument exceptions (issue#41)
        [InlineData(-1, 0)]
        [InlineData(0, -1)]
        [InlineData(-1, -1)]
        public void ConstructWithMajorMinorTest(int major, int minor)
        {
            var v = new SemVersion(major, minor);

            Assert.Equal(major, v.Major);
            Assert.Equal(minor, v.Minor);
            Assert.Equal(0, v.Patch);
            Assert.Equal("", v.Prerelease);
            Assert.Empty(v.PrereleaseIdentifiers);
            Assert.Equal("", v.Metadata);
            Assert.Empty(v.MetadataIdentifiers);
        }
        #endregion

        #region SemVersion(int major, int minor, int patch)
        [Theory]
        [InlineData(1, 2, 3)]
        // TODO Negative versions should be invalid and throw argument exceptions (issue#41)
        [InlineData(-1, 0, 0)]
        [InlineData(0, -1, 0)]
        [InlineData(0, 0, -1)]
        [InlineData(-1, -1, -1)]
        public void ConstructWithMajorMinorPatchTest(int major, int minor, int patch)
        {
            var v = new SemVersion(major, minor, patch);

            Assert.Equal(major, v.Major);
            Assert.Equal(minor, v.Minor);
            Assert.Equal(patch, v.Patch);
            Assert.Equal("", v.Prerelease);
            Assert.Empty(v.PrereleaseIdentifiers);
            Assert.Equal("", v.Metadata);
            Assert.Empty(v.MetadataIdentifiers);
        }
        #endregion

        #region SemVersion(int major, int minor = 0, int patch = 0, IEnumerable<PrereleaseIdentifier> prerelease = null, IEnumerable<MetadataIdentifier> metadata = null)
        [Fact]
        public void ConstructWithIdentifiersTest()
        {
            var prereleaseIdentifiers = new[] { new PrereleaseIdentifier("pre"), new PrereleaseIdentifier(42) };
            var metadata = new[] { new MetadataIdentifier("build"), new MetadataIdentifier("42") };
            var v = new SemVersion(1, 2, 3, prereleaseIdentifiers, metadata);

            Assert.Equal(1, v.Major);
            Assert.Equal(2, v.Minor);
            Assert.Equal(3, v.Patch);
            Assert.Equal("pre.42", v.Prerelease);
            Assert.Equal(prereleaseIdentifiers, v.PrereleaseIdentifiers);
            Assert.Equal("build.42", v.Metadata);
            Assert.Equal(metadata, v.MetadataIdentifiers);
        }

        [Theory]
        [InlineData(-1, 0, 0, InvalidMajorVersionMessage, "major")]
        [InlineData(0, -1, 0, InvalidMinorVersionMessage, "minor")]
        [InlineData(0, 0, -1, InvalidPatchVersionMessage, "patch")]
        [InlineData(-1, -1, -1, InvalidMajorVersionMessage, "major")]
        [InlineData(0, -1, -1, InvalidMinorVersionMessage, "minor")]
        public void ConstructWithIdentifiersInvalidTest(int major, int minor, int patch, string expectedMessage, string expectedParamName)
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(()
                => new SemVersion(major, minor, patch, Enumerable.Empty<PrereleaseIdentifier>()));

            Assert.StartsWith(expectedMessage, ex.Message);
            Assert.Equal(expectedParamName, ex.ParamName);
        }

        [Fact]
        public void ConstructWithDefaultPrereleaseIdentifiersTest()
        {
            var prereleaseIdentifiers = new[] { new PrereleaseIdentifier() };
            var ex = Assert.Throws<ArgumentException>(() => new SemVersion(1, 2, 3, prereleaseIdentifiers));

            Assert.StartsWith(PrereleaseIdentifierIsDefaultMessage, ex.Message);
            Assert.Equal("prerelease", ex.ParamName);
        }

        [Fact]
        public void ConstructWithDefaultMetadataIdentifiersTest()
        {
            var metadataIdentifiers = new[] { new MetadataIdentifier() };
            var ex = Assert.Throws<ArgumentException>(() => new SemVersion(1, 2, 3, metadata: metadataIdentifiers));

            Assert.StartsWith(MetadataIdentifierIsDefaultMessage, ex.Message);
            Assert.Equal("metadata", ex.ParamName);
        }
        #endregion

        #region SemVersion(int major, int minor = 0, int patch = 0, IEnumerable<string> prerelease = null, IEnumerable<string> metadata = null)
        [Fact]
        public void ConstructWithStringIdentifiersTest()
        {
            var prereleaseIdentifiers = new[] { "pre", "42" };
            var metadata = new[] { "build", "42" };
            var v = new SemVersion(1, 2, 3, prereleaseIdentifiers, metadata);

            Assert.Equal(1, v.Major);
            Assert.Equal(2, v.Minor);
            Assert.Equal(3, v.Patch);
            Assert.Equal("pre.42", v.Prerelease);
            var expectedPrereleaseIdentifiers = new[] { new PrereleaseIdentifier("pre"), new PrereleaseIdentifier(42) };
            Assert.Equal(expectedPrereleaseIdentifiers, v.PrereleaseIdentifiers);
            Assert.Equal("build.42", v.Metadata);
            var expectedMetadata = new[] { new MetadataIdentifier("build"), new MetadataIdentifier("42") };
            Assert.Equal(expectedMetadata, v.MetadataIdentifiers);
        }

        [Theory]
        [InlineData(-1, 0, 0, InvalidMajorVersionMessage, "major")]
        [InlineData(0, -1, 0, InvalidMinorVersionMessage, "minor")]
        [InlineData(0, 0, -1, InvalidPatchVersionMessage, "patch")]
        [InlineData(-1, -1, -1, InvalidMajorVersionMessage, "major")]
        [InlineData(0, -1, -1, InvalidMinorVersionMessage, "minor")]
        public void ConstructWithStringIdentifiersInvalidTest(int major, int minor, int patch, string expectedMessage, string expectedParamName)
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new SemVersion(major, minor, patch, Enumerable.Empty<string>()));

            Assert.StartsWith(expectedMessage, ex.Message);
            Assert.Equal(expectedParamName, ex.ParamName);
        }

        [Fact]
        public void ConstructWithStringIdentifiersPrereleaseNull()
        {
            var v = new SemVersion(1, 2, 3, (IEnumerable<string>)null);

            Assert.Equal(new SemVersion(1, 2, 3), v);
        }

        [Fact]
        public void ConstructWithStringIdentifiersPrereleaseEmpty()
        {
            var v = new SemVersion(1, 2, 3, Enumerable.Empty<string>());

            Assert.Equal(new SemVersion(1, 2, 3), v);
        }

        [Fact]
        public void ConstructWithStringIdentifiersPrereleaseEmptyIdentifier()
        {
            var ex = Assert.Throws<ArgumentException>(()
                => new SemVersion(1, 2, 3, new[] { "bar", "" }));
            Assert.StartsWith("Prerelease identifier cannot be empty.", ex.Message);
            Assert.Equal("prerelease", ex.ParamName);
        }

        [Fact]
        public void ConstructWithStringIdentifiersPrereleaseNullIdentifier()
        {
            var ex = Assert.Throws<ArgumentNullException>(()
                => new SemVersion(1, 2, 3, new[] { "bar", null }));
            Assert.StartsWith(ExceptionMessages.NotNull, ex.Message);
            Assert.Equal("prerelease", ex.ParamName);
        }

        [Fact]
        public void ConstructWithStringIdentifiersPrereleaseLeadingZeros()
        {
            var ex = Assert.Throws<ArgumentException>(()
                => new SemVersion(1, 2, 3, new[] { "bar", "0123" }));
            Assert.StartsWith("Leading zeros are not allowed on numeric prerelease identifiers '0123'.", ex.Message);
            Assert.Equal("prerelease", ex.ParamName);
        }

        [Fact]
        public void ConstructWithStringIdentifiersPrereleaseTooLarge()
        {
            var ex = Assert.Throws<OverflowException>(()
                => new SemVersion(1, 2, 3, new[] { "bar", "99999999999999999" }));
            Assert.StartsWith("Prerelease identifier '99999999999999999' was too large for Int32.", ex.Message);
        }

        [Fact]
        public void ConstructWithStringIdentifiersPrereleaseInvalidCharacter()
        {
            var ex = Assert.Throws<ArgumentException>(()
                => new SemVersion(1, 2, 3, new[] { "bar", "abc@123" }));
            Assert.StartsWith("A prerelease identifier can contain only ASCII alphanumeric characters and hyphens 'abc@123'.", ex.Message);
            Assert.Equal("prerelease", ex.ParamName);
        }

        [Fact]
        public void ConstructWithStringIdentifiersMetadataNull()
        {
            var v = new SemVersion(1, 2, 3, metadata: (IEnumerable<string>)null);

            Assert.Equal(new SemVersion(1, 2, 3), v);
        }

        [Fact]
        public void ConstructWithStringIdentifiersMetadataEmpty()
        {
            var v = new SemVersion(1, 2, 3, metadata: Enumerable.Empty<string>());

            Assert.Equal(new SemVersion(1, 2, 3), v);
        }

        [Fact]
        public void ConstructWithStringIdentifiersMetadataEmptyIdentifier()
        {
            var ex = Assert.Throws<ArgumentException>(()
                => new SemVersion(1, 2, 3, metadata: new[] { "bar", "" }));
            Assert.StartsWith("Metadata identifier cannot be empty.", ex.Message);
            Assert.Equal("metadata", ex.ParamName);
        }

        [Fact]
        public void ConstructWithStringIdentifiersMetadataNullIdentifier()
        {
            var ex = Assert.Throws<ArgumentNullException>(()
                => new SemVersion(1, 2, 3, metadata: new[] { "bar", null }));
            Assert.StartsWith(ExceptionMessages.NotNull, ex.Message);
            Assert.Equal("metadata", ex.ParamName);
        }

        [Fact]
        public void ConstructWithStringIdentifiersMetadataLeadingZeros()
        {
            var v = new SemVersion(1, 2, 3, metadata: new[] { "bar", "0123" });

            Assert.Equal(SemVersion.ParsedFrom(1, 2, 3, "", "bar.0123"), v);
        }

        [Fact]
        public void ConstructWithStringIdentifiersMetadataTooLarge()
        {
            var v = new SemVersion(1, 2, 3, metadata: new[] { "bar", "99999999999999999" });

            Assert.Equal(SemVersion.ParsedFrom(1, 2, 3, "", "bar.99999999999999999"), v);
        }

        [Fact]
        public void ConstructWithStringIdentifiersMetadataInvalidCharacter()
        {
            var ex = Assert.Throws<ArgumentException>(()
                => new SemVersion(1, 2, 3, metadata: new[] { "bar", "abc@123" }));
            Assert.StartsWith(
                "A metadata identifier can contain only ASCII alphanumeric characters and hyphens 'abc@123'.",
                ex.Message);
            Assert.Equal("metadata", ex.ParamName);
        }
        #endregion

        #region SemVersion.ParsedFrom(int major, int minor = 0, int patch = 0, string prerelease, string build)
        [Fact]
        public void ParsedFromDefaultValuesTest()
        {
            var v = SemVersion.ParsedFrom(1);

            Assert.Equal(1, v.Major);
            Assert.Equal(0, v.Minor);
            Assert.Equal(0, v.Patch);
            Assert.Equal("", v.Prerelease);
            Assert.Empty(v.PrereleaseIdentifiers);
            Assert.Equal("", v.Metadata);
            Assert.Empty(v.MetadataIdentifiers);
        }

        [Fact]
        public void ParsedFromTest()
        {
            var v = SemVersion.ParsedFrom(2, 3, 4, "pre.42", "build.42");

            Assert.Equal(2, v.Major);
            Assert.Equal(3, v.Minor);
            Assert.Equal(4, v.Patch);
            Assert.Equal("pre.42", v.Prerelease);
            var expectedPrereleaseIdentifiers = new[] { new PrereleaseIdentifier("pre"), new PrereleaseIdentifier(42) };
            Assert.Equal(expectedPrereleaseIdentifiers, v.PrereleaseIdentifiers);
            Assert.Equal("build.42", v.Metadata);
            var expectedMetadata = new[] { new MetadataIdentifier("build"), new MetadataIdentifier("42") };
            Assert.Equal(expectedMetadata, v.MetadataIdentifiers);
        }

        [Theory]
        [InlineData(-1, 0, 0, InvalidMajorVersionMessage, "major")]
        [InlineData(0, -1, 0, InvalidMinorVersionMessage, "minor")]
        [InlineData(0, 0, -1, InvalidPatchVersionMessage, "patch")]
        [InlineData(-1, -1, -1, InvalidMajorVersionMessage, "major")]
        [InlineData(0, -1, -1, InvalidMinorVersionMessage, "minor")]
        public void ParsedFromInvalidTest(
            int major,
            int minor,
            int patch,
            string expectedMessage,
            string expectedParamName)
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(()
                => SemVersion.ParsedFrom(major, minor, patch));

            Assert.StartsWith(expectedMessage, ex.Message);
            Assert.Equal(expectedParamName, ex.ParamName);
        }

        [Fact]
        public void ParsedFromPrereleaseEmptyString()
        {
            var v = SemVersion.ParsedFrom(1, 2, 3, "");

            Assert.Equal(new SemVersion(1, 2, 3), v);
        }

        [Fact]
        public void ParsedFromPrereleaseEmptyIdentifier()
        {
            var ex = Assert.Throws<ArgumentException>(() => SemVersion.ParsedFrom(1, 2, 3, "bar."));
            Assert.StartsWith("Prerelease identifier cannot be empty.", ex.Message);
            Assert.Equal("prerelease", ex.ParamName);
        }

        [Fact]
        public void ParsedFromPrereleaseLeadingZeros()
        {
            var ex = Assert.Throws<ArgumentException>(()
                => SemVersion.ParsedFrom(1, 2, 3, "bar.0123"));
            Assert.StartsWith("Leading zeros are not allowed on numeric prerelease identifiers '0123'.", ex.Message);
            Assert.Equal("prerelease", ex.ParamName);
        }

        [Fact]
        public void ParsedFromPrereleaseLeadingZerosAllowed()
        {
            var v = SemVersion.ParsedFrom(1, 2, 3, "bar.0123", allowLeadingZeros: true);

            Assert.Equal(SemVersion.ParsedFrom(1, 2, 3, "bar.123"), v);
        }

        [Fact]
        public void ParsedFromPrereleaseTooLarge()
        {
            var ex = Assert.Throws<OverflowException>(()
               => SemVersion.ParsedFrom(1, 2, 3, "bar.99999999999999999"));
            Assert.StartsWith("Prerelease identifier '99999999999999999' was too large for Int32.", ex.Message);
        }

        [Fact]
        public void ParsedFromPrereleaseInvalidCharacter()
        {
            var ex = Assert.Throws<ArgumentException>(()
                => SemVersion.ParsedFrom(1, 2, 3, "bar.abc@123"));
            Assert.StartsWith("A prerelease identifier can contain only ASCII alphanumeric characters and hyphens 'abc@123'.", ex.Message);
            Assert.Equal("prerelease", ex.ParamName);
        }

        [Fact]
        public void ParsedFromMetadataEmptyString()
        {
            var v = SemVersion.ParsedFrom(1, metadata: "");

            Assert.Equal(new SemVersion(1, 0, 0), v);
        }

        [Fact]
        public void ParsedFromMetadataEmptyIdentifier()
        {
            var ex = Assert.Throws<ArgumentException>(()
                => SemVersion.ParsedFrom(1, metadata: "bar."));
            Assert.StartsWith("Metadata identifier cannot be empty.", ex.Message);
            Assert.Equal("metadata", ex.ParamName);
        }

        [Fact]
        public void ParsedFromMetadataLeadingZeros()
        {
            var v = SemVersion.ParsedFrom(1, metadata: "bar.0123");

            Assert.Equal(SemVersion.ParsedFrom(1, 0, 0, "", "bar.0123"), v);
        }

        [Fact]
        public void ParsedFromMetadataTooLarge()
        {
            var v = SemVersion.ParsedFrom(1, metadata: "bar.99999999999999999");

            Assert.Equal(SemVersion.ParsedFrom(1, 0, 0, "", "bar.99999999999999999"), v);
        }

        [Fact]
        public void ParsedFromMetadataInvalidCharacter()
        {
            var ex = Assert.Throws<ArgumentException>(()
                => SemVersion.ParsedFrom(1, metadata: "bar.abc@123"));
            Assert.StartsWith("A metadata identifier can contain only ASCII alphanumeric characters and hyphens 'abc@123'.", ex.Message);
            Assert.Equal("metadata", ex.ParamName);
        }
        #endregion
    }
}
