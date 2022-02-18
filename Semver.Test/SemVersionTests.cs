using System;
using System.Globalization;
using System.Linq;
#if !NETSTANDARD
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
#endif
using Xunit;

namespace Semver.Test
{
    /// <summary>
    /// Tests of basic <see cref="SemVersion"/> functionality.
    /// </summary>
    public class SemVersionTests
    {
        #region Constructors
        /// <summary>
        /// Verifies the default values of the arguments to the primary constructor.
        /// </summary>
        [Fact]
        public void ConstructSemVersionDefaultValuesTest()
        {
            var v = new SemVersion(1);

            Assert.Equal(1, v.Major);
            Assert.Equal(0, v.Minor);
            Assert.Equal(0, v.Patch);
            Assert.Equal("", v.Prerelease);
            Assert.Empty(v.PrereleaseIdentifiers);
            Assert.Equal("", v.Metadata);
            Assert.Empty(v.MetadataIdentifiers);
        }

        [Theory]
        // Basic version
        [InlineData(1, 2, 3, "a", "b")]
        // Letter Limits
        [InlineData(1, 2, 3, "A-Z.a-z.0-9", "A-Z.a-z.0-9")]
        // Dash in strange places
        [InlineData(1, 2, 3, "-", "b")]
        [InlineData(1, 2, 3, "--", "b")]
        [InlineData(1, 2, 3, "a", "-")]
        [InlineData(1, 2, 3, "a", "--")]
        [InlineData(1, 2, 3, "-a", "b")]
        [InlineData(1, 2, 3, "--a", "b")]
        [InlineData(1, 2, 3, "a", "-b")]
        [InlineData(1, 2, 3, "a", "--b")]
        [InlineData(1, 2, 3, "a-", "b")]
        [InlineData(1, 2, 3, "a--", "b")]
        [InlineData(1, 2, 3, "a", "b-")]
        [InlineData(1, 2, 3, "a", "b--")]
        [InlineData(1, 2, 3, "-.a", "b")]
        [InlineData(1, 2, 3, "a", "-.b")]
        [InlineData(1, 2, 3, "a.-", "b")]
        [InlineData(1, 2, 3, "a.-.c", "b")]
        [InlineData(1, 2, 3, "a", "b.-")]
        [InlineData(1, 2, 3, "a", "b.-.c")]
        // Leading Zero on prerelease Alphanumeric Identifiers
        [InlineData(1, 2, 3, "0a", "b")]
        [InlineData(1, 2, 3, "00000a", "b")]
        [InlineData(1, 2, 3, "a.0c", "b")]
        [InlineData(1, 2, 3, "a.00000c", "b")]
        // Empty string
        [InlineData(1, 2, 3, "a", "")]
        [InlineData(1, 2, 3, "", "b")]
        [InlineData(1, 2, 3, "", "")]
        // Null handling
        [InlineData(1, 2, 3, "a", null)]
        [InlineData(1, 2, 3, null, "b")]
        [InlineData(1, 2, 3, null, null)]
        // Negative version numbers
        // TODO Negative versions should be invalid and throw argument exceptions (issue#41)
        [InlineData(-1, 0, 0, "", "")]
        [InlineData(0, -1, 0, "", "")]
        [InlineData(0, 0, -1, "", "")]
        [InlineData(-1, -1, -1, "", "")]
        // Illegal characters
        // TODO Illegal characters should be invalid and throw argument exceptions (issue#41)
        [InlineData(1, 2, 3, "😞", "b")]
        [InlineData(1, 2, 3, "a", "😞")]
        // Leading Zeros in Prerelease
        // TODO Leading zeros in prerelease should be invalid and throw argument exceptions (issue#41)
        [InlineData(1, 2, 3, "01", "b")]
        [InlineData(1, 2, 3, "a.01", "b")]
        [InlineData(1, 2, 3, "a.01.c", "b")]
        [InlineData(1, 2, 3, "a.0000001.c", "b")]
        // Leading Zeros in MetaData (valid)
        [InlineData(1, 2, 3, "a", "01")]
        [InlineData(1, 2, 3, "a", "b.01")]
        [InlineData(1, 2, 3, "a", "b.01.c")]
        [InlineData(1, 2, 3, "a", "b.00000001.c")]
        [InlineData(1, 2, 3, "a", "0b")]
        [InlineData(1, 2, 3, "a", "0000000b")]
        [InlineData(1, 2, 3, "a", "b.0c")]
        [InlineData(1, 2, 3, "a", "b.000000c")]
        // Empty Identifiers
        // TODO Empty Identifiers should be invalid and throw argument exceptions (issue#41)
        [InlineData(1, 2, 3, ".", "b")]
        [InlineData(1, 2, 3, "a", ".")]
        [InlineData(1, 2, 3, "a.", "b")]
        [InlineData(1, 2, 3, "a..", "b")]
        [InlineData(1, 2, 3, "a", "b.")]
        [InlineData(1, 2, 3, "a", "b..")]
        [InlineData(1, 2, 3, ".a", "b")]
        [InlineData(1, 2, 3, "..a", "b")]
        [InlineData(1, 2, 3, "a", ".b")]
        [InlineData(1, 2, 3, "a", "..b")]
        [InlineData(1, 2, 3, "a..c", "b")]
        [InlineData(1, 2, 3, "a", "b..c")]
        public void ConstructSemVersionTest(int major, int minor, int patch, string prerelease, string metadata)
        {
            var v = new SemVersion(major, minor, patch, prerelease, metadata);

            Assert.Equal(major, v.Major);
            Assert.Equal(minor, v.Minor);
            Assert.Equal(patch, v.Patch);
            Assert.Equal(prerelease ?? "", v.Prerelease);
#pragma warning disable CS0612 // Type or member is obsolete
            var expectedPrereleaseIdentifiers =
                (prerelease?.SplitExceptEmpty('.') ?? Enumerable.Empty<string>())
                            .Select(PrereleaseIdentifier.CreateLoose);
#pragma warning restore CS0612 // Type or member is obsolete
            Assert.Equal(expectedPrereleaseIdentifiers, v.PrereleaseIdentifiers);
            Assert.Equal(metadata ?? "", v.Metadata);
            var expectedMetadataIdentifiers = metadata?.SplitExceptEmpty('.') ?? Enumerable.Empty<string>();
            Assert.Equal(expectedMetadataIdentifiers, v.MetadataIdentifiers);
        }
        #endregion

        [Theory]
        [InlineData(1, 2, 3, "a", "b")]
        [InlineData(1, 2, 3, "A-Z.a-z.0-9", "A-Z.a-z.0-9")]
        [InlineData(1, 2, 3, "a", "😞")]
        [InlineData(1, 2, 3, "a", "b..c")]
        [InlineData(1, 2, 3, "a", null)]
        [InlineData(1, 2, 3, "a", "-")]
        public void BuildTest(int major, int minor, int patch, string prerelease, string metadata)
        {
            var v = new SemVersion(major, minor, patch, prerelease, metadata);

#pragma warning disable 618
            Assert.Equal(metadata ?? "", v.Build);
#pragma warning restore 618
        }

        [Theory]
        [InlineData(1, 2, 3, "A-Z.a-z.0-9", "A-Z.a-z.0-9", true)]
        [InlineData(1, 2, 3, "-", "b", true)]
        [InlineData(1, 2, 3, ".", "b", true)]
        [InlineData(1, 2, 3, "..", "b", true)]
        [InlineData(1, 2, 3, "01", "b", true)]
        [InlineData(1, 2, 3, "😞", "b", true)]
        [InlineData(1, 2, 3, "", "b", false)]
        [InlineData(1, 2, 3, null, "b", false)]
        public void IsPrereleaseTest(int major, int minor, int patch, string prerelease, string metadata, bool expected)
        {
            var v = new SemVersion(major, minor, patch, prerelease, metadata);

            Assert.True(expected == v.IsPrerelease, v.ToString());
        }

        #region System.Version
        [Theory]
        [InlineData(0, 0, 0, 0)]
        [InlineData(1, 1, 1, 1)]
        [InlineData(1, 2, 0, 3)]
        [InlineData(1, 2, 4, 3)]
        public void ConstructSemVersionFromSystemVersionTest(int major, int minor, int build, int revision)
        {
            var nonSemanticVersion = new Version(major, minor, build, revision);

#pragma warning disable 618
            var v = new SemVersion(nonSemanticVersion);
#pragma warning restore 618

            Assert.Equal(major, v.Major);
            Assert.Equal(minor, v.Minor);
            Assert.Equal(revision, v.Patch);
            Assert.Equal("", v.Prerelease);
            Assert.Empty(v.PrereleaseIdentifiers);
            if (build > 0)
            {
                var metadata = build.ToString(CultureInfo.InvariantCulture);
                Assert.Equal(metadata, v.Metadata);
                Assert.Equal(new[] { metadata }, v.MetadataIdentifiers);
            }
            else
            {
                Assert.Equal("", v.Metadata);
                Assert.Empty(v.MetadataIdentifiers);
            }
        }

        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(1, 1, 1)]
        [InlineData(1, 2, 0)]
        [InlineData(1, 2, 4)]
        public void ConstructSemVersionFromSystemVersionWithUndefinedRevisionTest(int major, int minor, int build)
        {
            var nonSemanticVersion = new Version(major, minor, build);

#pragma warning disable 618
            var v = new SemVersion(nonSemanticVersion);
#pragma warning restore 618

            Assert.Equal(major, v.Major);
            Assert.Equal(minor, v.Minor);
            Assert.Equal(0, v.Patch);
            Assert.Equal("", v.Prerelease);
            Assert.Empty(v.PrereleaseIdentifiers);
            if (build > 0)
            {
                var metadata = build.ToString(CultureInfo.InvariantCulture);
                Assert.Equal(metadata, v.Metadata);
                Assert.Equal(new[] { metadata }, v.MetadataIdentifiers);
            }
            else
            {
                Assert.Equal("", v.Metadata);
                Assert.Empty(v.MetadataIdentifiers);
            }
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 1)]
        [InlineData(1, 2)]
        public void ConstructSemVersionFromSystemVersionWithUndefinedBuildRevisionTest(int major, int minor)
        {
            var nonSemanticVersion = new Version(major, minor);

#pragma warning disable 618
            var v = new SemVersion(nonSemanticVersion);
#pragma warning restore 618

            Assert.Equal(major, v.Major);
            Assert.Equal(minor, v.Minor);
            Assert.Equal(0, v.Patch);
            Assert.Equal("", v.Prerelease);
            Assert.Empty(v.PrereleaseIdentifiers);
            Assert.Equal("", v.Metadata);
            Assert.Empty(v.MetadataIdentifiers);
        }

        [Fact]
        public void ConstructSemVersionFromNullSystemVersionTest()
        {
#pragma warning disable 618
            var ex = Assert.Throws<ArgumentNullException>(() => new SemVersion(null));
#pragma warning restore 618

            Assert.StartsWith("Value cannot be null.", ex.Message);
            Assert.Equal("version", ex.ParamName);
        }

        [Theory]
        [InlineData(0, 0, 0, 0)]
        [InlineData(1, 1, 1, 0)]
        [InlineData(1, 2, 0, 0)]
        [InlineData(45, 2, 4, 0)]
        public void FromVersionWithZeroRevisionTest(int major, int minor, int build, int revision)
        {
            var nonSemanticVersion = new Version(major, minor, build, revision);

            var v = SemVersion.FromVersion(nonSemanticVersion);

            Assert.Equal(major, v.Major);
            Assert.Equal(minor, v.Minor);
            Assert.Equal(build, v.Patch);
            Assert.Equal("", v.Prerelease);
            Assert.Empty(v.PrereleaseIdentifiers);
            Assert.Equal("", v.Metadata);
            Assert.Empty(v.MetadataIdentifiers);
        }

        [Theory]
        [InlineData(1, 1, 1, 1)]
        [InlineData(1, 2, 0, 32)]
        [InlineData(45, 2, 4, 5414)]
        public void FromVersionWithPositiveRevisionTest(int major, int minor, int build, int revision)
        {
            var nonSemanticVersion = new Version(major, minor, build, revision);

            var ex = Assert.Throws<ArgumentException>(() => SemVersion.FromVersion(nonSemanticVersion));

            Assert.StartsWith("Version with Revision number can't be converted to SemVer.", ex.Message);
            Assert.Equal("version", ex.ParamName);
        }

        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(1, 1, 1)]
        [InlineData(1, 2, 0)]
        [InlineData(22, 2, 4)]
        public void FromVersionWithUndefinedRevisionTest(int major, int minor, int build)
        {
            var nonSemanticVersion = new Version(major, minor, build);

            var v = SemVersion.FromVersion(nonSemanticVersion);

            Assert.Equal(major, v.Major);
            Assert.Equal(minor, v.Minor);
            Assert.Equal(build, v.Patch);
            Assert.Equal("", v.Prerelease);
            Assert.Empty(v.PrereleaseIdentifiers);
            Assert.Equal("", v.Metadata);
            Assert.Empty(v.MetadataIdentifiers);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 1)]
        [InlineData(1, 2)]
        [InlineData(12, 62)]
        public void FromVersionWithUndefinedBuildRevisionTest(int major, int minor)
        {
            var nonSemanticVersion = new Version(major, minor);

            var v = SemVersion.FromVersion(nonSemanticVersion);

            Assert.Equal(major, v.Major);
            Assert.Equal(minor, v.Minor);
            Assert.Equal(0, v.Patch);
            Assert.Equal("", v.Prerelease);
            Assert.Empty(v.PrereleaseIdentifiers);
            Assert.Equal("", v.Metadata);
            Assert.Empty(v.MetadataIdentifiers);
        }

        [Fact]
        public void FromVersionNullTest()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => SemVersion.FromVersion(null));

            Assert.StartsWith("Value cannot be null.", ex.Message);
            Assert.Equal("version", ex.ParamName);
        }

        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(1, 2, 3)]
        [InlineData(34, 62, 0)]
        public void ToVersionFromReleaseTest(int major, int minor, int patch)
        {
            var v = new SemVersion(major, minor, patch);

            var nonSemanticVersion = v.ToVersion();

            Assert.Equal(major, nonSemanticVersion.Major);
            Assert.Equal(minor, nonSemanticVersion.Minor);
            Assert.Equal(patch, nonSemanticVersion.Build);
            Assert.Equal(-1, nonSemanticVersion.Revision);
        }

        [Theory]
        [InlineData(-1, 0, 0, "A-Z.a-z.0-9", "A-Z.a-z.0-9")]
        [InlineData(0, -1, 0, "", "")]
        [InlineData(0, 0, -1, "alpha", "")]
        [InlineData(-1, -1, -1, "", "build.42")]
        public void ToVersionFromNegativeTest(int major, int minor, int patch, string prerelease, string metadata)
        {
            var v = new SemVersion(major, minor, patch, prerelease, metadata);

            var ex = Assert.Throws<InvalidOperationException>(() => v.ToVersion());

            Assert.Equal("Negative version numbers can't be converted to System.Version.", ex.Message);
        }

        [Theory]
        [InlineData(1, 2, 3, "A-Z.a-z.0-9", "A-Z.a-z.0-9")]
        [InlineData(1, 2, 3, "-", "b")]
        [InlineData(1, 2, 3, ".", "b")]
        [InlineData(1, 2, 3, "..", "b")]
        [InlineData(1, 2, 3, "01", "b")]
        [InlineData(1, 2, 3, "😞", "b")]
        public void ToVersionFromPrereleaseTest(int major, int minor, int patch, string prerelease, string metadata)
        {
            var v = new SemVersion(major, minor, patch, prerelease, metadata);

            var ex = Assert.Throws<InvalidOperationException>(() => v.ToVersion());

            Assert.Equal("Prerelease version can't be converted to System.Version.", ex.Message);
        }

        [Theory]
        [InlineData(1, 2, 3, "", "A-Z.a-z.0-9")]
        [InlineData(1, 2, 3, "", "😞")]
        [InlineData(1, 2, 3, "", ".")]
        [InlineData(1, 2, 3, "", "..b")]
        [InlineData(1, 2, 3, "", "-")]
        [InlineData(1, 2, 3, "", "b..c")]
        public void ToVersionFromMetadataTest(int major, int minor, int patch, string prerelease, string metadata)
        {
            var v = new SemVersion(major, minor, patch, prerelease, metadata);

            var ex = Assert.Throws<InvalidOperationException>(() => v.ToVersion());

            Assert.Equal("Version with build metadata can't be converted to System.Version.", ex.Message);
        }
        #endregion

        [Theory]
        [InlineData(1, 2, 3, "a", "b", "1.2.3-a+b")]
        [InlineData(1, 2, 3, "a", "", "1.2.3-a")]
        [InlineData(1, 2, 3, "", "b", "1.2.3+b")]
        [InlineData(1, 2, 3, "", "", "1.2.3")]
        [InlineData(1, 2, 0, "", "", "1.2.0")]
        [InlineData(1, 0, 0, "", "", "1.0.0")]
        [InlineData(0, 0, 0, "", "", "0.0.0")]
        [InlineData(6, 20, 31, "beta-x.2", "dev-mha.120", "6.20.31-beta-x.2+dev-mha.120")]
        [InlineData(-1, 0, 0, "", "", "-1.0.0")]
        [InlineData(0, -1, 0, "", "", "0.-1.0")]
        [InlineData(0, 0, -1, "", "", "0.0.-1")]
        [InlineData(-1, -1, -1, "", "", "-1.-1.-1")]
        public void ToStringTest(int major, int minor, int patch, string prerelease, string metadata, string expected)
        {
            var v = new SemVersion(major, minor, patch, prerelease, metadata);

            var actual = v.ToString();

            Assert.Equal(expected, actual);
        }

        #region Change

        // TODO add tests for validation
        [Fact]
        public void ChangeMajorTest()
        {
            var v1 = new SemVersion(1, 2, 3, "alpha", "dev");
            var v2 = v1.Change(major: 5);

            Assert.Equal(5, v2.Major);
            Assert.Equal(2, v2.Minor);
            Assert.Equal(3, v2.Patch);
            Assert.Equal("alpha", v2.Prerelease);
            Assert.Equal("dev", v2.Metadata);
        }

        // TODO add tests for validation
        [Fact]
        public void ChangeMinorTest()
        {
            var v1 = new SemVersion(1, 2, 3, "alpha", "dev");
            var v2 = v1.Change(minor: 5);

            Assert.Equal(1, v2.Major);
            Assert.Equal(5, v2.Minor);
            Assert.Equal(3, v2.Patch);
            Assert.Equal("alpha", v2.Prerelease);
            Assert.Equal("dev", v2.Metadata);
        }

        // TODO add tests for validation
        [Fact]
        public void ChangePatchTest()
        {
            var v1 = new SemVersion(1, 2, 3, "alpha", "dev");
            var v2 = v1.Change(patch: 5);

            Assert.Equal(1, v2.Major);
            Assert.Equal(2, v2.Minor);
            Assert.Equal(5, v2.Patch);
            Assert.Equal("alpha", v2.Prerelease);
            Assert.Equal("dev", v2.Metadata);
        }

        // TODO add tests for validation
        [Fact]
        public void ChangePrereleaseTest()
        {
            var v1 = new SemVersion(1, 2, 3, "alpha", "dev");
            var v2 = v1.Change(prerelease: "beta");

            Assert.Equal(1, v2.Major);
            Assert.Equal(2, v2.Minor);
            Assert.Equal(3, v2.Patch);
            Assert.Equal("beta", v2.Prerelease);
            Assert.Equal("dev", v2.Metadata);
        }

        // TODO add tests for validation
        [Fact]
        public void ChangeBuildTest()
        {
            var v1 = new SemVersion(1, 2, 3, "alpha", "dev");
            // TODO this parameter should be named 'metadata', but that is a breaking change
            var v2 = v1.Change(build: "gamma");

            Assert.Equal(1, v2.Major);
            Assert.Equal(2, v2.Minor);
            Assert.Equal(3, v2.Patch);
            Assert.Equal("alpha", v2.Prerelease);
            Assert.Equal("gamma", v2.Metadata);
        }
        #endregion

#if !NETSTANDARD
        [Fact]
        public void SerializationTest()
        {
            var semVer = new SemVersion(1, 2, 3, "alpha", "dev");
            SemVersion semVerSerializedDeserialized;
            using (var ms = new MemoryStream())
            {
                var bf = new BinaryFormatter();
                bf.Serialize(ms, semVer);
                ms.Position = 0;
                semVerSerializedDeserialized = (SemVersion)bf.Deserialize(ms);
            }
            Assert.Equal(semVer, semVerSerializedDeserialized);
            Assert.Equal(semVer.PrereleaseIdentifiers, semVerSerializedDeserialized.PrereleaseIdentifiers);
            Assert.Equal(semVer.MetadataIdentifiers, semVerSerializedDeserialized.MetadataIdentifiers);
        }
#endif
    }
}