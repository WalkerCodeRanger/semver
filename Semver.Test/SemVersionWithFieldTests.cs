using System;
using System.Collections.Generic;
using System.Linq;
using Semver.Test.Builders;
using Semver.Test.Helpers;
using Xunit;

namespace Semver.Test
{
    /// <summary>
    /// Tests of the "With..." methods of <see cref="SemVersion"/>.
    /// </summary>
    public class SemVersionWithFieldTests
    {
        public static readonly SemVersion Version = SemVersion.ParsedFrom(1, 2, 3, "pre", "metadata");
        public static readonly SemVersion ReleaseVersion = SemVersion.ParsedFrom(1, 2, 3, "", "metadata");
        public static readonly SemVersion NoMetadataVersion = SemVersion.ParsedFrom(1, 2, 3, "pre");
        public static readonly SemVersion ReleaseWithoutMetadataVersion = SemVersion.ParsedFrom(1, 2, 3);

        #region WithMajor, WithMinor, WithPatch
        [Fact]
        public void WithMajor()
        {
            var v = Version.WithMajor(42);

            Assert.Equal(SemVersion.ParsedFrom(42, 2, 3, "pre", "metadata"), v);
        }

        [Fact]
        public void WithSameMajor()
        {
            var v = Version.WithMajor(1);

            Assert.Same(Version, v);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public void WithMajorInvalid(int majorVersion)
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => Version.WithMajor(majorVersion));
            Assert.StartsWith("Major version must be greater than or equal to zero.", ex.Message);
            Assert.Equal("major", ex.ParamName);
        }

        [Fact]
        public void WithMinor()
        {
            var v = Version.WithMinor(42);

            Assert.Equal(SemVersion.ParsedFrom(1, 42, 3, "pre", "metadata"), v);
        }

        [Fact]
        public void WithSameMinor()
        {
            var v = Version.WithMinor(2);

            Assert.Same(Version, v);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public void WithMinorInvalid(int minorVersion)
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => Version.WithMinor(minorVersion));
            Assert.StartsWith("Minor version must be greater than or equal to zero.", ex.Message);
            Assert.Equal("minor", ex.ParamName);
        }

        [Fact]
        public void WithPatch()
        {
            var v = Version.WithPatch(42);

            Assert.Equal(SemVersion.ParsedFrom(1, 2, 42, "pre", "metadata"), v);
        }

        [Fact]
        public void WithSamePatch()
        {
            var v = Version.WithPatch(3);

            Assert.Same(Version, v);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public void WithPatchInvalid(int patchVersion)
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => Version.WithPatch(patchVersion));
            Assert.StartsWith("Patch version must be greater than or equal to zero.", ex.Message);
            Assert.Equal("patch", ex.ParamName);
        }
        #endregion

        #region WithPrereleaseParsedFrom(string, bool)
        [Fact]
        public void WithPrereleaseParsedFrom()
        {
            var v = Version.WithPrereleaseParsedFrom("bar.baz.100.123abc");

            Assert.Equal(SemVersion.ParsedFrom(1, 2, 3, "bar.baz.100.123abc", "metadata"), v);
        }

        [Fact]
        public void WithPrereleaseParsedFromAlreadyRelease()
        {
            var v = ReleaseVersion.WithPrereleaseParsedFrom("");

            Assert.Same(ReleaseVersion, v);
        }

        [Fact]
        public void WithPrereleaseParsedFromNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(()
                => Version.WithPrereleaseParsedFrom(null));
            Assert.StartsWith(ExceptionMessages.NotNull, ex.Message);
            Assert.Equal("prerelease", ex.ParamName);
        }

        [Fact]
        public void WithPrereleaseParsedFromEmptyString()
        {
            var v = Version.WithPrereleaseParsedFrom("");

            Assert.Equal(SemVersion.ParsedFrom(1, 2, 3, "", "metadata"), v);
        }

        [Fact]
        public void WithPrereleaseParsedFromEmptyIdentifier()
        {
            var ex = Assert.Throws<ArgumentException>(()
                => Version.WithPrereleaseParsedFrom("bar."));
            Assert.StartsWith("Prerelease identifier cannot be empty.", ex.Message);
            Assert.Equal("prerelease", ex.ParamName);
        }

        [Fact]
        public void WithPrereleaseParsedFromLeadingZeros()
        {
            var ex = Assert.Throws<ArgumentException>(()
                => Version.WithPrereleaseParsedFrom("bar.0123"));
            Assert.StartsWith("Leading zeros are not allowed on numeric prerelease identifiers '0123'.", ex.Message);
            Assert.Equal("prerelease", ex.ParamName);
        }

        [Fact]
        public void WithPrereleaseParsedFromLeadingZerosAllowed()
        {
            var v = Version.WithPrereleaseParsedFrom("bar.0123", allowLeadingZeros: true);

            Assert.Equal(SemVersion.ParsedFrom(1, 2, 3, "bar.123", "metadata"), v);
        }

        [Fact]
        public void WithPrereleaseParsedFromTooLarge()
        {
            var ex = Assert.Throws<OverflowException>(()
               => Version.WithPrereleaseParsedFrom("bar.99999999999999999"));
            Assert.StartsWith("Prerelease identifier '99999999999999999' was too large for Int32.", ex.Message);
        }

        [Fact]
        public void WithPrereleaseParsedFromInvalidCharacter()
        {
            var ex = Assert.Throws<ArgumentException>(()
                => Version.WithPrereleaseParsedFrom("bar.abc@123"));
            Assert.StartsWith("A prerelease identifier can contain only ASCII alphanumeric characters and hyphens 'abc@123'.", ex.Message);
            Assert.Equal("prerelease", ex.ParamName);
        }
        #endregion

        #region WithPrerelease(string, params string[])
        [Fact]
        public void WithPrereleaseStringParams()
        {
            var v = Version.WithPrerelease("bar", "baz", "100", "123abc");

            Assert.Equal(SemVersion.ParsedFrom(1, 2, 3, "bar.baz.100.123abc", "metadata"), v);
        }

        [Fact]
        public void WithPrereleaseStringParamsFirstNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => Version.WithPrerelease(null, "more"));
            Assert.StartsWith(ExceptionMessages.NotNull, ex.Message);
            Assert.Equal("prereleaseIdentifier", ex.ParamName);
        }

        [Fact]
        public void WithPrereleaseStringParamsRestNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => Version.WithPrerelease("pre", null));
            Assert.StartsWith(ExceptionMessages.NotNull, ex.Message);
            Assert.Equal("prereleaseIdentifiers", ex.ParamName);
        }

        [Fact]
        public void WithPrereleaseStringParamsEmptyIdentifierInFirst()
        {
            var ex = Assert.Throws<ArgumentException>(() => Version.WithPrerelease("", "bax"));
            Assert.StartsWith("Prerelease identifier cannot be empty.", ex.Message);
            Assert.Equal("prereleaseIdentifiers", ex.ParamName);
        }

        [Fact]
        public void WithPrereleaseStringParamsEmptyIdentifierInRest()
        {
            var ex = Assert.Throws<ArgumentException>(() => Version.WithPrerelease("bar", ""));
            Assert.StartsWith("Prerelease identifier cannot be empty.", ex.Message);
            Assert.Equal("prereleaseIdentifiers", ex.ParamName);
        }

        [Fact]
        public void WithPrereleaseStringParamsNullIdentifierInRest()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => Version.WithPrerelease("bar", "baz", null));
            Assert.StartsWith(ExceptionMessages.NotNull, ex.Message);
            Assert.Equal("prereleaseIdentifiers", ex.ParamName);
        }

        [Fact]
        public void WithPrereleaseStringParamsLeadingZerosInFirst()
        {
            var ex = Assert.Throws<ArgumentException>(() => Version.WithPrerelease("0123", "bar"));
            Assert.StartsWith("Leading zeros are not allowed on numeric prerelease identifiers '0123'.", ex.Message);
            Assert.Equal("prereleaseIdentifiers", ex.ParamName);
        }

        [Fact]
        public void WithPrereleaseStringParamsLeadingZerosInRest()
        {
            var ex = Assert.Throws<ArgumentException>(() => Version.WithPrerelease("bar", "0123"));
            Assert.StartsWith("Leading zeros are not allowed on numeric prerelease identifiers '0123'.", ex.Message);
            Assert.Equal("prereleaseIdentifiers", ex.ParamName);
        }

        [Fact]
        public void WithPrereleaseStringParamsTooLargeInFirst()
        {
            var ex = Assert.Throws<OverflowException>(() => Version.WithPrerelease("99999999999999999", "bar"));
            Assert.StartsWith("Prerelease identifier '99999999999999999' was too large for Int32.", ex.Message);
        }

        [Fact]
        public void WithPrereleaseStringParamsTooLargeInRest()
        {
            var ex = Assert.Throws<OverflowException>(()
                => Version.WithPrerelease("bar", "99999999999999999"));
            Assert.StartsWith("Prerelease identifier '99999999999999999' was too large for Int32.", ex.Message);
        }

        [Fact]
        public void WithPrereleaseStringParamsInvalidCharacterInFirst()
        {
            var ex = Assert.Throws<ArgumentException>(() => Version.WithPrerelease("abc@123", "bar"));
            Assert.StartsWith(
                "A prerelease identifier can contain only ASCII alphanumeric characters and hyphens 'abc@123'.",
                ex.Message);
            Assert.Equal("prereleaseIdentifiers", ex.ParamName);
        }

        [Fact]
        public void WithPrereleaseStringParamsInvalidCharacterInRest()
        {
            var ex = Assert.Throws<ArgumentException>(() => Version.WithPrerelease("bar", "abc@123"));
            Assert.StartsWith("A prerelease identifier can contain only ASCII alphanumeric characters and hyphens 'abc@123'.", ex.Message);
            Assert.Equal("prereleaseIdentifiers", ex.ParamName);
        }
        #endregion

        #region WithPrerelease(IEnumerable<string>)
        [Fact]
        public void WithPrereleaseStringEnumerable()
        {
            var v = Version.WithPrerelease(new[] { "bar", "baz", "100", "123abc" });

            Assert.Equal(SemVersion.ParsedFrom(1, 2, 3, "bar.baz.100.123abc", "metadata"), v);
        }

        [Fact]
        public void WithPrereleaseEnumerableAlreadyRelease()
        {
            var v = ReleaseVersion.WithPrerelease(Enumerable.Empty<string>());

            Assert.Same(ReleaseVersion, v);
        }

        [Fact]
        public void WithPrereleaseStringEnumerableNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(()
                => Version.WithPrerelease((IEnumerable<string>)null));
            Assert.StartsWith(ExceptionMessages.NotNull, ex.Message);
            Assert.Equal("prereleaseIdentifiers", ex.ParamName);
        }

        [Fact]
        public void WithPrereleaseStringEnumerableEmpty()
        {
            var v = Version.WithPrerelease(Enumerable.Empty<string>());

            Assert.Equal(SemVersion.ParsedFrom(1, 2, 3, "", "metadata"), v);
        }

        [Fact]
        public void WithPrereleaseStringEnumerableEmptyIdentifier()
        {
            var ex = Assert.Throws<ArgumentException>(()
                => Version.WithPrerelease(new[] { "bar", "" }));
            Assert.StartsWith("Prerelease identifier cannot be empty.", ex.Message);
            Assert.Equal("prereleaseIdentifiers", ex.ParamName);
        }

        [Fact]
        public void WithPrereleaseStringEnumerableNullIdentifier()
        {
            var ex = Assert.Throws<ArgumentNullException>(()
                => Version.WithPrerelease(new[] { "bar", null }));
            Assert.StartsWith(ExceptionMessages.NotNull, ex.Message);
            Assert.Equal("prereleaseIdentifiers", ex.ParamName);
        }

        [Fact]
        public void WithPrereleaseStringEnumerableLeadingZeros()
        {
            var ex = Assert.Throws<ArgumentException>(()
                => Version.WithPrerelease(new[] { "bar", "0123" }));
            Assert.StartsWith("Leading zeros are not allowed on numeric prerelease identifiers '0123'.", ex.Message);
            Assert.Equal("prereleaseIdentifiers", ex.ParamName);
        }

        [Fact]
        public void WithPrereleaseStringEnumerableTooLarge()
        {
            var ex = Assert.Throws<OverflowException>(()
                => Version.WithPrerelease(new[] { "bar", "99999999999999999" }));
            Assert.StartsWith("Prerelease identifier '99999999999999999' was too large for Int32.", ex.Message);
        }

        [Fact]
        public void WithPrereleaseStringEnumerableInvalidCharacter()
        {
            var ex = Assert.Throws<ArgumentException>(()
                => Version.WithPrerelease(new[] { "bar", "abc@123" }));
            Assert.StartsWith("A prerelease identifier can contain only ASCII alphanumeric characters and hyphens 'abc@123'.", ex.Message);
            Assert.Equal("prereleaseIdentifiers", ex.ParamName);
        }
        #endregion

        #region WithPrerelease(PrereleaseIdentifier, params PrereleaseIdentifier[])
        [Fact]
        public void WithPrereleaseIdentifierParams()
        {
            var v = Version.WithPrerelease(
                new PrereleaseIdentifier("bar"),
                new PrereleaseIdentifier("baz"),
                new PrereleaseIdentifier("100"),
                new PrereleaseIdentifier("123abc"));

            Assert.Equal(SemVersion.ParsedFrom(1, 2, 3, "bar.baz.100.123abc", "metadata"), v);
        }

        [Fact]
        public void WithPrereleaseIdentifierParamsRestNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => Version.WithPrerelease(
                new PrereleaseIdentifier("bar"), null));
            Assert.StartsWith(ExceptionMessages.NotNull, ex.Message);
            Assert.Equal("prereleaseIdentifiers", ex.ParamName);
        }

        [Fact]
        public void WithPrereleaseIdentifierParamsDefaultIdentifierInFirst()
        {
            var ex = Assert.Throws<ArgumentException>(() => Version.WithPrerelease(
                default, new PrereleaseIdentifier("bar")));
            Assert.StartsWith("Prerelease identifier cannot be default/null.", ex.Message);
            Assert.Equal("prereleaseIdentifiers", ex.ParamName);
        }

        [Fact]
        public void WithPrereleaseIdentifierParamsDefaultIdentifierInRest()
        {
            var ex = Assert.Throws<ArgumentException>(() => Version.WithPrerelease(
                new PrereleaseIdentifier("bar"), new PrereleaseIdentifier("baz"), default));
            Assert.StartsWith("Prerelease identifier cannot be default/null.", ex.Message);
            Assert.Equal("prereleaseIdentifiers", ex.ParamName);
        }
        #endregion

        #region WithPrerelease(IEnumerable<PrereleaseIdentifier>)
        [Fact]
        public void WithPrereleaseIdentifiers()
        {
            var v = Version.WithPrerelease(new[]
            {
                new PrereleaseIdentifier("bar"),
                new PrereleaseIdentifier("baz"),
                new PrereleaseIdentifier("100"),
                new PrereleaseIdentifier("123abc")
            });

            Assert.Equal(SemVersion.ParsedFrom(1, 2, 3, "bar.baz.100.123abc", "metadata"), v);
        }

        [Fact]
        public void WithPrereleaseIdentifiersAlreadyRelease()
        {
            var v = ReleaseVersion.WithPrerelease(Enumerable.Empty<PrereleaseIdentifier>());

            Assert.Same(ReleaseVersion, v);
        }

        [Fact]
        public void WithPrereleaseIdentifiersNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(()
                => Version.WithPrerelease((IEnumerable<PrereleaseIdentifier>)null));
            Assert.StartsWith(ExceptionMessages.NotNull, ex.Message);
            Assert.Equal("prereleaseIdentifiers", ex.ParamName);
        }

        [Fact]
        public void WithPrereleaseIdentifiersEmpty()
        {
            var v = Version.WithPrerelease(Enumerable.Empty<PrereleaseIdentifier>());

            Assert.Equal(SemVersion.ParsedFrom(1, 2, 3, "", "metadata"), v);
        }

        [Fact]
        public void WithPrereleaseIdentifiersDefaultIdentifier()
        {
            var ex = Assert.Throws<ArgumentException>(() => Version.WithPrerelease(new[]
            {
                new PrereleaseIdentifier("bar"), default
            }));
            Assert.StartsWith("Prerelease identifier cannot be default/null.", ex.Message);
            Assert.Equal("prereleaseIdentifiers", ex.ParamName);
        }
        #endregion

        [Fact]
        public void WithoutPrerelease()
        {
            var v = Version.WithoutPrerelease();

            Assert.Equal(SemVersion.ParsedFrom(1, 2, 3, "", "metadata"), v);
        }

        [Fact]
        public void WithoutPrereleaseAlreadyRelease()
        {
            var v = ReleaseVersion.WithoutPrerelease();

            Assert.Same(ReleaseVersion, v);
        }

        #region WithMetadataParsedFrom(string)
        [Fact]
        public void WithMetadataParsedFrom()
        {
            var v = Version.WithMetadataParsedFrom("bar.baz.100.123abc");

            Assert.Equal(SemVersion.ParsedFrom(1, 2, 3, "pre", "bar.baz.100.123abc"), v);
        }

        [Fact]
        public void WithMetadataParsedFromAlreadyNoMetadata()
        {
            var v = NoMetadataVersion.WithMetadataParsedFrom("");

            Assert.Same(NoMetadataVersion, v);
        }

        [Fact]
        public void WithMetadataParsedFromNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(()
                => Version.WithMetadataParsedFrom(null));
            Assert.StartsWith(ExceptionMessages.NotNull, ex.Message);
            Assert.Equal("metadata", ex.ParamName);
        }

        [Fact]
        public void WithMetadataParsedFromEmptyString()
        {
            var v = Version.WithMetadataParsedFrom("");

            Assert.Equal(SemVersion.ParsedFrom(1, 2, 3, "pre"), v);
        }

        [Fact]
        public void WithMetadataParsedFromEmptyIdentifier()
        {
            var ex = Assert.Throws<ArgumentException>(()
                => Version.WithMetadataParsedFrom("bar."));
            Assert.StartsWith("Metadata identifier cannot be empty.", ex.Message);
            Assert.Equal("metadata", ex.ParamName);
        }

        [Fact]
        public void WithMetadataParsedFromLeadingZeros()
        {
            var v = Version.WithMetadataParsedFrom("bar.0123");

            Assert.Equal(SemVersion.ParsedFrom(1, 2, 3, "pre", "bar.0123"), v);
        }

        [Fact]
        public void WithMetadataParsedFromTooLarge()
        {
            var v = Version.WithMetadataParsedFrom("bar.99999999999999999");

            Assert.Equal(SemVersion.ParsedFrom(1, 2, 3, "pre", "bar.99999999999999999"), v);
        }

        [Fact]
        public void WithMetadataParsedFromInvalidCharacter()
        {
            var ex = Assert.Throws<ArgumentException>(()
                => Version.WithMetadataParsedFrom("bar.abc@123"));
            Assert.StartsWith("A metadata identifier can contain only ASCII alphanumeric characters and hyphens 'abc@123'.", ex.Message);
            Assert.Equal("metadata", ex.ParamName);
        }
        #endregion

        #region WithMetadata(string, params string[])
        [Fact]
        public void WithMetadataStringParams()
        {
            var v = Version.WithMetadata("bar", "baz", "100", "123abc");

            Assert.Equal(SemVersion.ParsedFrom(1, 2, 3, "pre", "bar.baz.100.123abc"), v);
        }

        [Fact]
        public void WithMetadataStringParamsFirstNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => Version.WithMetadata(null, "rest"));
            Assert.StartsWith(ExceptionMessages.NotNull, ex.Message);
            Assert.Equal("metadataIdentifiers", ex.ParamName);
        }

        [Fact]
        public void WithMetadataStringParamsRestNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => Version.WithMetadata("bar", null));
            Assert.StartsWith(ExceptionMessages.NotNull, ex.Message);
            Assert.Equal("metadataIdentifiers", ex.ParamName);
        }

        [Fact]
        public void WithMetadataStringParamsEmptyIdentifierInFirst()
        {
            var ex = Assert.Throws<ArgumentException>(() => Version.WithMetadata("", "bar"));
            Assert.StartsWith("Metadata identifier cannot be empty.", ex.Message);
            Assert.Equal("metadataIdentifiers", ex.ParamName);
        }

        [Fact]
        public void WithMetadataStringParamsEmptyIdentifierInRest()
        {
            var ex = Assert.Throws<ArgumentException>(() => Version.WithMetadata("bar", ""));
            Assert.StartsWith("Metadata identifier cannot be empty.", ex.Message);
            Assert.Equal("metadataIdentifiers", ex.ParamName);
        }

        [Fact]
        public void WithMetadataStringParamsNullIdentifierInRest()
        {
            var ex = Assert.Throws<ArgumentNullException>(()
                => Version.WithMetadata("bar", "baz", null));
            Assert.StartsWith(ExceptionMessages.NotNull, ex.Message);
            Assert.Equal("metadataIdentifiers", ex.ParamName);
        }

        [Fact]
        public void WithMetadataStringParamsLeadingZerosInFirst()
        {
            var v = Version.WithMetadata("0123", "bar");

            Assert.Equal(SemVersion.ParsedFrom(1, 2, 3, "pre", "0123.bar"), v);
        }

        [Fact]
        public void WithMetadataStringParamsLeadingZerosInRest()
        {
            var v = Version.WithMetadata("bar", "0123");

            Assert.Equal(SemVersion.ParsedFrom(1, 2, 3, "pre", "bar.0123"), v);
        }

        [Fact]
        public void WithMetadataStringParamsTooLargeInFirst()
        {
            var v = Version.WithMetadata("99999999999999999", "bar");

            Assert.Equal(SemVersion.ParsedFrom(1, 2, 3, "pre", "99999999999999999.bar"), v);
        }

        [Fact]
        public void WithMetadataStringParamsTooLargeInRest()
        {
            var v = Version.WithMetadata("bar", "99999999999999999");

            Assert.Equal(SemVersion.ParsedFrom(1, 2, 3, "pre", "bar.99999999999999999"), v);
        }

        [Fact]
        public void WithMetadataStringParamsInvalidCharacterInFirst()
        {
            var ex = Assert.Throws<ArgumentException>(() => Version.WithMetadata("abc@123", "bar"));
            Assert.StartsWith(
                "A metadata identifier can contain only ASCII alphanumeric characters and hyphens 'abc@123'.",
                ex.Message);
            Assert.Equal("metadataIdentifiers", ex.ParamName);
        }

        [Fact]
        public void WithMetadataStringParamsInvalidCharacterInRest()
        {
            var ex = Assert.Throws<ArgumentException>(() => Version.WithMetadata("bar", "abc@123"));
            Assert.StartsWith(
                "A metadata identifier can contain only ASCII alphanumeric characters and hyphens 'abc@123'.",
                ex.Message);
            Assert.Equal("metadataIdentifiers", ex.ParamName);
        }
        #endregion

        #region WithMetadata(IEnumerable<string>)
        [Fact]
        public void WithMetadataEnumerable()
        {
            var v = Version.WithMetadata(new[] { "bar", "baz", "100", "123abc" });

            Assert.Equal(SemVersion.ParsedFrom(1, 2, 3, "pre", "bar.baz.100.123abc"), v);
        }

        [Fact]
        public void WithMetadataEnumerableAlreadyNoMetadata()
        {
            var v = NoMetadataVersion.WithMetadata(Enumerable.Empty<string>());

            Assert.Same(NoMetadataVersion, v);
        }

        [Fact]
        public void WithMetadataEnumerableNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(()
                => Version.WithMetadata((IEnumerable<string>)null));
            Assert.StartsWith(ExceptionMessages.NotNull, ex.Message);
            Assert.Equal("metadataIdentifiers", ex.ParamName);
        }

        [Fact]
        public void WithMetadataEnumerableEmpty()
        {
            var v = Version.WithMetadata(Enumerable.Empty<string>());

            Assert.Equal(SemVersion.ParsedFrom(1, 2, 3, "pre"), v);
        }

        [Fact]
        public void WithMetadataEnumerableEmptyIdentifier()
        {
            var ex = Assert.Throws<ArgumentException>(()
                => Version.WithMetadata(new[] { "bar", "" }));
            Assert.StartsWith("Metadata identifier cannot be empty.", ex.Message);
            Assert.Equal("metadataIdentifiers", ex.ParamName);
        }

        [Fact]
        public void WithMetadataEnumerableNullIdentifier()
        {
            var ex = Assert.Throws<ArgumentNullException>(()
                => Version.WithMetadata(new[] { "bar", null }));
            Assert.StartsWith(ExceptionMessages.NotNull, ex.Message);
            Assert.Equal("metadataIdentifiers", ex.ParamName);
        }

        [Fact]
        public void WithMetadataEnumerableLeadingZeros()
        {
            var v = Version.WithMetadata(new[] { "bar", "0123" });

            Assert.Equal(SemVersion.ParsedFrom(1, 2, 3, "pre", "bar.0123"), v);
        }

        [Fact]
        public void WithMetadataEnumerableTooLarge()
        {
            var v = Version.WithMetadata(new[] { "bar", "99999999999999999" });

            Assert.Equal(SemVersion.ParsedFrom(1, 2, 3, "pre", "bar.99999999999999999"), v);
        }

        [Fact]
        public void WithMetadataEnumerableInvalidCharacter()
        {
            var ex = Assert.Throws<ArgumentException>(()
                => Version.WithMetadata(new[] { "bar", "abc@123" }));
            Assert.StartsWith(
                "A metadata identifier can contain only ASCII alphanumeric characters and hyphens 'abc@123'.",
                ex.Message);
            Assert.Equal("metadataIdentifiers", ex.ParamName);
        }
        #endregion

        #region WithMetadata(MetadataIdentifier, params MetadataIdentifier[])
        [Fact]
        public void WithMetadataIdentifierParams()
        {
            var v = Version.WithMetadata(
                new MetadataIdentifier("bar"),
                new MetadataIdentifier("baz"),
                new MetadataIdentifier("100"),
                new MetadataIdentifier("123abc"));

            Assert.Equal(SemVersion.ParsedFrom(1, 2, 3, "pre", "bar.baz.100.123abc"), v);
        }

        [Fact]
        public void WithMetadataIdentifierParamsRestNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(()
                => Version.WithMetadata(new MetadataIdentifier("bar"), null));
            Assert.StartsWith(ExceptionMessages.NotNull, ex.Message);
            Assert.Equal("metadataIdentifiers", ex.ParamName);
        }

        [Fact]
        public void WithMetadataIdentifierParamsDefaultIdentifierInFirst()
        {
            var ex = Assert.Throws<ArgumentException>(()
                => Version.WithMetadata(default, new MetadataIdentifier("bar")));
            Assert.StartsWith("Metadata identifier cannot be default/null.", ex.Message);
            Assert.Equal("metadataIdentifiers", ex.ParamName);
        }

        [Fact]
        public void WithMetadataIdentifierParamsDefaultIdentifierInRest()
        {
            var ex = Assert.Throws<ArgumentException>(() => Version.WithMetadata(
                new MetadataIdentifier("bar"), new MetadataIdentifier("baz"), default));
            Assert.StartsWith("Metadata identifier cannot be default/null.", ex.Message);
            Assert.Equal("metadataIdentifiers", ex.ParamName);
        }
        #endregion

        #region WithMetadata(IEnumerable<MetadataIdentifier>)
        [Fact]
        public void WithMetadataIdentifiers()
        {
            var v = Version.WithMetadata(new[]
            {
                new MetadataIdentifier("bar"),
                new MetadataIdentifier("baz"),
                new MetadataIdentifier("100"),
                new MetadataIdentifier("123abc")
            });

            Assert.Equal(SemVersion.ParsedFrom(1, 2, 3, "pre", "bar.baz.100.123abc"), v);
        }

        [Fact]
        public void WithMetadataIdentifiersAlreadyNoMetadata()
        {
            var v = NoMetadataVersion.WithMetadata(Enumerable.Empty<MetadataIdentifier>());

            Assert.Same(NoMetadataVersion, v);
        }

        [Fact]
        public void WithMetadataIdentifiersNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(()
                => Version.WithMetadata((IEnumerable<MetadataIdentifier>)null));
            Assert.StartsWith(ExceptionMessages.NotNull, ex.Message);
            Assert.Equal("metadataIdentifiers", ex.ParamName);
        }

        [Fact]
        public void WithMetadataIdentifiersEmpty()
        {
            var v = Version.WithMetadata(Enumerable.Empty<MetadataIdentifier>());

            Assert.Equal(SemVersion.ParsedFrom(1, 2, 3, "pre"), v);
        }

        [Fact]
        public void WithMetadataIdentifiersDefaultIdentifier()
        {
            var ex = Assert.Throws<ArgumentException>(() => Version.WithMetadata(new[]
            {
                new MetadataIdentifier("bar"), default
            }));
            Assert.StartsWith("Metadata identifier cannot be default/null.", ex.Message);
            Assert.Equal("metadataIdentifiers", ex.ParamName);
        }
        #endregion

        [Fact]
        public void WithoutMetadata()
        {
            var v = Version.WithoutMetadata();

            Assert.Equal(SemVersion.ParsedFrom(1, 2, 3, "pre"), v);
        }

        [Fact]
        public void WithoutMetadataAlreadyNoMetadata()
        {
            var v = NoMetadataVersion.WithoutMetadata();

            Assert.Same(NoMetadataVersion, v);
        }

        [Fact]
        public void WithoutPrereleaseOrMetadata()
        {
            var v = Version.WithoutPrereleaseOrMetadata();

            Assert.Equal(new SemVersion(1, 2, 3), v);
        }

        [Fact]
        public void WithoutPrereleaseOrMetadataAlreadyReleaseWithoutMetadata()
        {
            var v = ReleaseWithoutMetadataVersion.WithoutPrereleaseOrMetadata();

            Assert.Same(ReleaseWithoutMetadataVersion, v);
        }
    }
}
