using System;
using System.Globalization;
using Xunit;

namespace Semver.Test
{
    /// <summary>
    /// Tests of obsolete methods on <see cref="SemVersion"/>
    /// </summary>
    public class SemVersionObsoleteTests
    {
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

#pragma warning disable CS0618 // Type or member is obsolete
            Assert.Equal(metadata ?? "", v.Build);
#pragma warning restore CS0618 // Type or member is obsolete
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
                Assert.Equal(new[] { new MetadataIdentifier(metadata) }, v.MetadataIdentifiers);
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
                Assert.Equal(new[] { new MetadataIdentifier(metadata) }, v.MetadataIdentifiers);
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
        #endregion

        #region Change
        [Fact]
        public void ChangeMajorTest()
        {
            var v1 = new SemVersion(1, 2, 3, "alpha", "dev");
#pragma warning disable CS0618 // Type or member is obsolete
            var v2 = v1.Change(major: 5);
#pragma warning restore CS0618 // Type or member is obsolete

            Assert.Equal(5, v2.Major);
            Assert.Equal(2, v2.Minor);
            Assert.Equal(3, v2.Patch);
            Assert.Equal("alpha", v2.Prerelease);
            Assert.Equal("dev", v2.Metadata);
        }

        [Fact]
        public void ChangeMinorTest()
        {
            var v1 = new SemVersion(1, 2, 3, "alpha", "dev");
#pragma warning disable CS0618 // Type or member is obsolete
            var v2 = v1.Change(minor: 5);
#pragma warning restore CS0618 // Type or member is obsolete

            Assert.Equal(1, v2.Major);
            Assert.Equal(5, v2.Minor);
            Assert.Equal(3, v2.Patch);
            Assert.Equal("alpha", v2.Prerelease);
            Assert.Equal("dev", v2.Metadata);
        }

        [Fact]
        public void ChangePatchTest()
        {
            var v1 = new SemVersion(1, 2, 3, "alpha", "dev");
#pragma warning disable CS0618 // Type or member is obsolete
            var v2 = v1.Change(patch: 5);
#pragma warning restore CS0618 // Type or member is obsolete

            Assert.Equal(1, v2.Major);
            Assert.Equal(2, v2.Minor);
            Assert.Equal(5, v2.Patch);
            Assert.Equal("alpha", v2.Prerelease);
            Assert.Equal("dev", v2.Metadata);
        }

        [Fact]
        public void ChangePrereleaseTest()
        {
            var v1 = new SemVersion(1, 2, 3, "alpha", "dev");
#pragma warning disable CS0618 // Type or member is obsolete
            var v2 = v1.Change(prerelease: "beta");
#pragma warning restore CS0618 // Type or member is obsolete

            Assert.Equal(1, v2.Major);
            Assert.Equal(2, v2.Minor);
            Assert.Equal(3, v2.Patch);
            Assert.Equal("beta", v2.Prerelease);
            Assert.Equal("dev", v2.Metadata);
        }

        [Fact]
        public void ChangeBuildTest()
        {
            var v1 = new SemVersion(1, 2, 3, "alpha", "dev");
            // This parameter should be named 'metadata', but that is a breaking change
#pragma warning disable CS0618 // Type or member is obsolete
            var v2 = v1.Change(build: "gamma");
#pragma warning restore CS0618 // Type or member is obsolete

            Assert.Equal(1, v2.Major);
            Assert.Equal(2, v2.Minor);
            Assert.Equal(3, v2.Patch);
            Assert.Equal("alpha", v2.Prerelease);
            Assert.Equal("gamma", v2.Metadata);
        }
        #endregion
    }
}
