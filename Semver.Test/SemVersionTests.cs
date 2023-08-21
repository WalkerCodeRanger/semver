﻿using System;
using System.IO;
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
            var ex = Assert.Throws<ArgumentNullException>(() => SemVersion.FromVersion(null!));

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

        [Theory]
        [InlineData("2147483648.2.3", "major")]
        [InlineData("1.2147483648.3", "minor")]
        [InlineData("1.2.2147483648", "patch")]
        public void ToVersionFromOverflowTest(string version, string kind)
        {
            var v = SemVersion.Parse(version);

            var ex = Assert.Throws<InvalidOperationException>(() => v.ToVersion());

            Assert.Equal($"Version with {kind} version of 2147483648 can't be converted to System.Version because it is greater than Int32.MaxValue.", ex.Message);
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

        [Fact]
        public void SerializationTest()
        {
            var semVer = SemVersion.ParsedFrom(1, 2, 3, "alpha", "dev");
            SemVersion semVerSerializedDeserialized;
            using (var ms = new MemoryStream())
            {
                var bf = new BinaryFormatter();
#pragma warning disable SYSLIB0011 // BinaryFormatter serialization is obsolete
                bf.Serialize(ms, semVer);
                ms.Position = 0;
                semVerSerializedDeserialized = (SemVersion)bf.Deserialize(ms);
#pragma warning restore SYSLIB0011
            }
            Assert.Equal(semVer, semVerSerializedDeserialized);
            Assert.Equal(semVer.PrereleaseIdentifiers, semVerSerializedDeserialized.PrereleaseIdentifiers);
            Assert.Equal(semVer.MetadataIdentifiers, semVerSerializedDeserialized.MetadataIdentifiers);
        }
    }
}