using System;
using System.Globalization;
#if !NETSTANDARD
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
#endif
using Xunit;

namespace Semver.Test
{
    public class SemVersionTests
    {
        #region Constructors
        [Fact]
        public void ConstructSemVersionDefaultValuesTest()
        {
            var v = new SemVersion(1);

            Assert.Equal(1, v.Major);
            Assert.Equal(0, v.Minor);
            Assert.Equal(0, v.Patch);
            Assert.Equal("", v.Prerelease);
            Assert.Equal("", v.Build);
        }

        [Theory]
        [InlineData(1, 2, 3, "a", "b")]
        [InlineData(1, 2, 3, null, null)]
        // TODO these should be invalid and throw argument exceptions issue #40
        [InlineData(-1, 0, 0, "", "")]
        [InlineData(0, -1, 0, "", "")]
        [InlineData(0, 0, -1, "", "")]
        // TODO these should be invalid and throw argument exceptions issue #41
        [InlineData(1, 2, 3, "😞", "b")]
        [InlineData(1, 2, 3, "a", "😞")]
        [InlineData(1, 2, 3, "01", "b")]
        [InlineData(1, 2, 3, "a.01", "b")]
        [InlineData(1, 2, 3, "a..empty", "b")]
        [InlineData(1, 2, 3, "a", "b..empty")]
        public void ConstructSemVersionTest(int major, int minor, int patch, string prerelease, string build)
        {
            var v = new SemVersion(major, minor, patch, prerelease, build);

            Assert.Equal(major, v.Major);
            Assert.Equal(minor, v.Minor);
            Assert.Equal(patch, v.Patch);
            Assert.Equal(prerelease ?? "", v.Prerelease);
            Assert.Equal(build ?? "", v.Build);
        }

        [Theory]
        [InlineData(0, 0, 0, 0)]
        // TODO this is a strange conversion (issue #32)
        [InlineData(1, 2, 0, 3)]
        [InlineData(1, 2, 4, 3)]
        public void ConstructSemVersionFromSystemVersionTest(int major, int minor, int build, int revision)
        {
            var nonSemanticVersion = new Version(major, minor, build, revision);

            var v = new SemVersion(nonSemanticVersion);

            Assert.Equal(major, v.Major);
            Assert.Equal(minor, v.Minor);
            Assert.Equal(revision, v.Patch);
            Assert.Equal("", v.Prerelease);
            Assert.Equal(build > 0 ? build.ToString(CultureInfo.InvariantCulture) : "", v.Build);
        }

        [Fact]
        public void ConstructSemVersionFromNullSystemVersionTest()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new SemVersion(null));

            Assert.Equal("Value cannot be null.\r\nParameter name: version", ex.Message);
        }
        #endregion

        [Fact]
        public void GetHashCodeTest()
        {
            var v1 = SemVersion.Parse("1.0.0-1+b");
            var v2 = SemVersion.Parse("1.0.0-1+c");

            var h1 = v1.GetHashCode();
            var h2 = v2.GetHashCode();

            Assert.NotEqual(h1, h2);
        }


        [Fact]
        public void ToStringTest()
        {
            var version = new SemVersion(1, 2, 3, "beta-x.2", "dev-mha.120");

            Assert.Equal("1.2.3-beta-x.2+dev-mha.120", version.ToString());
        }

        [Fact]
        public void ImplicitConversionFromStringTest()
        {
            SemVersion v = "1.0.0";
            Assert.Equal(1, v.Major);
        }

        #region Change
        [Fact]
        public void ChangeMajorTest()
        {
            var v1 = new SemVersion(1, 2, 3, "alpha", "dev");
            var v2 = v1.Change(major: 5);

            Assert.Equal(5, v2.Major);
            Assert.Equal(2, v2.Minor);
            Assert.Equal(3, v2.Patch);
            Assert.Equal("alpha", v2.Prerelease);
            Assert.Equal("dev", v2.Build);
        }

        [Fact]
        public void ChangeMinorTest()
        {
            var v1 = new SemVersion(1, 2, 3, "alpha", "dev");
            var v2 = v1.Change(minor: 5);

            Assert.Equal(1, v2.Major);
            Assert.Equal(5, v2.Minor);
            Assert.Equal(3, v2.Patch);
            Assert.Equal("alpha", v2.Prerelease);
            Assert.Equal("dev", v2.Build);
        }

        [Fact]
        public void ChangePatchTest()
        {
            var v1 = new SemVersion(1, 2, 3, "alpha", "dev");
            var v2 = v1.Change(patch: 5);

            Assert.Equal(1, v2.Major);
            Assert.Equal(2, v2.Minor);
            Assert.Equal(5, v2.Patch);
            Assert.Equal("alpha", v2.Prerelease);
            Assert.Equal("dev", v2.Build);
        }

        [Fact]
        public void ChangePrereleaseTest()
        {
            var v1 = new SemVersion(1, 2, 3, "alpha", "dev");
            var v2 = v1.Change(prerelease: "beta");

            Assert.Equal(1, v2.Major);
            Assert.Equal(2, v2.Minor);
            Assert.Equal(3, v2.Patch);
            Assert.Equal("beta", v2.Prerelease);
            Assert.Equal("dev", v2.Build);
        }

        [Fact]
        public void ChangeBuildTest()
        {
            var v1 = new SemVersion(1, 2, 3, "alpha", "dev");
            var v2 = v1.Change(build: "gamma");

            Assert.Equal(1, v2.Major);
            Assert.Equal(2, v2.Minor);
            Assert.Equal(3, v2.Patch);
            Assert.Equal("alpha", v2.Prerelease);
            Assert.Equal("gamma", v2.Build);
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
        }
#endif
    }
}