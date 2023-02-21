using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Semver.Test.Helpers;
using Xunit;

namespace Semver.Test
{
    /// <summary>
    /// Tests of basic <see cref="SemVersion"/> functionality.
    /// </summary>
    public class SemVersionTests
    {
        [Theory]
        [InlineData("1.2.3-A-Z.a-z.0-9+A-Z.a-z.0-9", true)]
        [InlineData("1.2.3--+b", true)]
        [InlineData("1.2.3-1+b", true)]
        [InlineData("1.2.3+b", false)]
        public void IsPrereleaseAndIsReleaseTest(string version, bool expected)
        {
            var v = SemVersion.Parse(version);

            Assert.True(expected == v.IsPrerelease, $"({v}).IsPrerelease");
            Assert.True(expected != v.IsRelease, $"({v}).IsRelease");
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

            Assert.StartsWith(ExceptionMessages.NotNull, ex.Message);
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
        [InlineData("1.2.3-A-Z.a-z.0-9+A-Z.a-z.0-9")]
        [InlineData("1.2.3--+b")]
        [InlineData("1.2.3-1+b")]
        public void ToVersionFromPrereleaseTest(string version)
        {
            var v = SemVersion.Parse(version);

            var ex = Assert.Throws<InvalidOperationException>(() => v.ToVersion());

            Assert.Equal("Prerelease version can't be converted to System.Version.", ex.Message);
        }

        [Theory]
        [InlineData("1.2.3+A-Z.a-z.0-9")]
        [InlineData("1.2.3+-")]
        public void ToVersionFromMetadataTest(string version)
        {
            var v = SemVersion.Parse(version);

            var ex = Assert.Throws<InvalidOperationException>(() => v.ToVersion());

            Assert.Equal("Version with build metadata can't be converted to System.Version.", ex.Message);
        }
        #endregion

        [Theory]
        [InlineData("1.2.3-a+b")]
        [InlineData("1.2.3-a")]
        [InlineData("1.2.3+b")]
        [InlineData("1.2.3")]
        [InlineData("1.2.0")]
        [InlineData("1.0.0")]
        [InlineData("0.0.0")]
        [InlineData("6.20.31-beta-x.2+dev-mha.120")]
        public void ToStringTest(string version)
        {
            var v = SemVersion.Parse(version);

            var actual = v.ToString();

            Assert.Equal(version, actual);
        }

#if SERIALIZABLE
        [Fact]
        public void SerializationTest()
        {
            var semVer = SemVersion.ParsedFrom(1, 2, 3, "alpha", "dev");
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
            var semVer = SemVersion.ParsedFrom(1, 2, 3, "alpha", "dev");
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