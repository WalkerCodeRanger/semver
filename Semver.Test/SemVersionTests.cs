using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Semver.Test.Builders;
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
        // Hyphen in strange places
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
            var expectedPrereleaseIdentifiers =
                (prerelease?.SplitExceptEmpty('.') ?? Enumerable.Empty<string>())
#pragma warning disable CS0612 // Type or member is obsolete
                    .Select(PrereleaseIdentifier.CreateLoose);
#pragma warning restore CS0612 // Type or member is obsolete
            Assert.Equal(expectedPrereleaseIdentifiers, v.PrereleaseIdentifiers);
            Assert.Equal(metadata ?? "", v.Metadata);
            var expectedMetadataIdentifiers =
                (metadata?.SplitExceptEmpty('.') ?? Enumerable.Empty<string>())
#pragma warning disable CS0612 // Type or member is obsolete
                    .Select(MetadataIdentifier.CreateLoose);
#pragma warning restore CS0612 // Type or member is obsolete
            ;
            Assert.Equal(expectedMetadataIdentifiers, v.MetadataIdentifiers);
        }
        #endregion

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

#if SERIALIZABLE
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
#else
        [Fact]
        public void SerializationNotSupportedTest()
        {
            var semVer = new SemVersion(1, 2, 3, "alpha", "dev");
            using (var ms = new MemoryStream())
            {
                var bf = new BinaryFormatter();
                var ex = Assert.Throws<SerializationException>(() => bf.Serialize(ms, semVer));
                // The CI build ends up with a different assembly name, so it can't be hardcoded
                var assemblyName = typeof(SemVersion).Assembly.FullName;
                string expectedMessage = $"Type 'Semver.SemVersion' in Assembly '{assemblyName}' is not marked as serializable.";
                Assert.Equal(expectedMessage, ex.Message);
            }
        }
#endif
    }
}