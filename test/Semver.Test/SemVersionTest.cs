using System;
using System.IO;
#if !NETSTANDARD
using System.Runtime.Serialization.Formatters.Binary;
#endif
using Xunit;

namespace Semver.Test
{
    public class SemverTests
    {
        [Fact]
        public void CompareTestWithStrings1()
        {
            Assert.True(SemVersion.Equals("1.0.0", "1"));
        }

        [Fact]
        public void CompareTestWithStrings2()
        {
            var v = new SemVersion(1, 0, 0);
            Assert.True(v < "1.1");
        }

        [Fact]
        public void CompareTestWithStrings3()
        {
            var v = new SemVersion(1, 2);
            Assert.True(v > "1.0.0");
        }

        [Fact]
        public void CreateVersionTest()
        {
            var v = new SemVersion(1, 2, 3, "a", "b");

            Assert.Equal(1, v.Major);
            Assert.Equal(2, v.Minor);
            Assert.Equal(3, v.Patch);
            Assert.Equal("a", v.Prerelease);
            Assert.Equal("b", v.Build);
        }

        [Fact]
        public void CreateVersionTestWithNulls()
        {
            var v = new SemVersion(1, 2, 3, null, null);

            Assert.Equal(1, v.Major);
            Assert.Equal(2, v.Minor);
            Assert.Equal(3, v.Patch);
            Assert.Equal("", v.Prerelease);
            Assert.Equal("", v.Build);
        }

        [Fact]
        public void CreateVersionTestWithSystemVersion1()
        {
            var nonSemanticVersion = new Version(0, 0);
            var v = new SemVersion(nonSemanticVersion);

            Assert.Equal(0, v.Major);
            Assert.Equal(0, v.Minor);
            Assert.Equal(0, v.Patch);
            Assert.Equal("", v.Build);
            Assert.Equal("", v.Prerelease);
        }

        [Fact]
        public void CreateVersionTestWithSystemVersion2()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var v = new SemVersion(null);
            });
        }

        [Fact]
        public void CreateVersionTestWithSystemVersion3()
        {
            var nonSemanticVersion = new Version(1, 2, 0, 3);
            var v = new SemVersion(nonSemanticVersion);

            Assert.Equal(1, v.Major);
            Assert.Equal(2, v.Minor);
            Assert.Equal(3, v.Patch);
            Assert.Equal("", v.Build);
            Assert.Equal("", v.Prerelease);
        }

        [Fact]
        public void CreateVersionTestWithSystemVersion4()
        {
            var nonSemanticVersion = new Version(1, 2, 4, 3);
            var v = new SemVersion(nonSemanticVersion);

            Assert.Equal(1, v.Major);
            Assert.Equal(2, v.Minor);
            Assert.Equal(3, v.Patch);
            Assert.Equal("4", v.Build);
            Assert.Equal("", v.Prerelease);
        }

        [Fact]
        public void ParseTest1()
        {
            var version = SemVersion.Parse("1.2.45-alpha+nightly.23");

            Assert.Equal(1, version.Major);
            Assert.Equal(2, version.Minor);
            Assert.Equal(45, version.Patch);
            Assert.Equal("alpha", version.Prerelease);
            Assert.Equal("nightly.23", version.Build);
        }

        [Fact]
        public void ParseTest2()
        {
            var version = SemVersion.Parse("1");

            Assert.Equal(1, version.Major);
            Assert.Equal(0, version.Minor);
            Assert.Equal(0, version.Patch);
            Assert.Equal("", version.Prerelease);
            Assert.Equal("", version.Build);
        }

        [Fact]
        public void ParseTest3()
        {
            var version = SemVersion.Parse("1.2.45-alpha-beta+nightly.23.43-bla");

            Assert.Equal(1, version.Major);
            Assert.Equal(2, version.Minor);
            Assert.Equal(45, version.Patch);
            Assert.Equal("alpha-beta", version.Prerelease);
            Assert.Equal("nightly.23.43-bla", version.Build);
        }

        [Fact]
        public void ParseTest4()
        {
            var version = SemVersion.Parse("2.0.0+nightly.23.43-bla");

            Assert.Equal(2, version.Major);
            Assert.Equal(0, version.Minor);
            Assert.Equal(0, version.Patch);
            Assert.Equal("", version.Prerelease);
            Assert.Equal("nightly.23.43-bla", version.Build);
        }

        [Fact]
        public void ParseTest5()
        {
            var version = SemVersion.Parse("2.0+nightly.23.43-bla");

            Assert.Equal(2, version.Major);
            Assert.Equal(0, version.Minor);
            Assert.Equal(0, version.Patch);
            Assert.Equal("", version.Prerelease);
            Assert.Equal("nightly.23.43-bla", version.Build);
        }

        [Fact]
        public void ParseTest6()
        {
            var version = SemVersion.Parse("2.1-alpha");

            Assert.Equal(2, version.Major);
            Assert.Equal(1, version.Minor);
            Assert.Equal(0, version.Patch);
            Assert.Equal("alpha", version.Prerelease);
            Assert.Equal("", version.Build);
        }

        [Fact]
        public void ParseTest7()
        {
            Assert.Throws<ArgumentException>(() => SemVersion.Parse("ui-2.1-alpha"));
        }

        [Fact]
        public void ParseTestStrict1()
        {
            var version = SemVersion.Parse("1.3.4", true);

            Assert.Equal(1, version.Major);
            Assert.Equal(3, version.Minor);
            Assert.Equal(4, version.Patch);
            Assert.Equal("", version.Prerelease);
            Assert.Equal("", version.Build);
        }

        [Fact]
        public void ParseTestStrict2()
        {
            Assert.Throws<InvalidOperationException>(() => SemVersion.Parse("1", true));
        }

        [Fact]
        public void ParseTestStrict3()
        {
            Assert.Throws<InvalidOperationException>(() => SemVersion.Parse("1.3", true));
        }

        [Fact]
        public void ParseTestStrict4()
        {
            Assert.Throws<InvalidOperationException>(() => SemVersion.Parse("1.3-alpha", true));
        }

        [Fact]
        public void TryParseTest1()
        {
            SemVersion v;
            Assert.True(SemVersion.TryParse("1.2.45-alpha-beta+nightly.23.43-bla", out v));
        }

        [Fact]
        public void TryParseTest2()
        {
            SemVersion v;
            Assert.False(SemVersion.TryParse("ui-2.1-alpha", out v));
        }

        [Fact]
        public void TryParseTest3()
        {
            SemVersion v;
            Assert.False(SemVersion.TryParse("", out v));
        }

        [Fact]
        public void TryParseTest4()
        {
            SemVersion v;
            Assert.False(SemVersion.TryParse(null, out v));
        }

        [Fact]
        public void TryParseTest5()
        {
            SemVersion v;
            Assert.True(SemVersion.TryParse("1.2", out v, false));
        }

        [Fact]
        public void TryParseTest6()
        {
            SemVersion v;
            Assert.False(SemVersion.TryParse("1.2", out v, true));
        }

        [Fact]
        public void ToStringTest()
        {
            var version = new SemVersion(1, 2, 0, "beta", "dev-mha.120");

            Assert.Equal("1.2.0-beta+dev-mha.120", version.ToString());
        }

        [Fact]
        public void EqualTest1()
        {
            var v1 = new SemVersion(1, 2, build: "nightly");
            var v2 = new SemVersion(1, 2, build: "nightly");

            var r = v1.Equals(v2);
            Assert.True(r);
        }

        [Fact]
        public void EqualTest2()
        {
            var v1 = new SemVersion(1, 2, prerelease: "alpha", build: "dev");
            var v2 = new SemVersion(1, 2, prerelease: "alpha", build: "dev");

            var r = v1.Equals(v2);
            Assert.True(r);
        }

        [Fact]
        public void EqualTest3()
        {
            var v1 = SemVersion.Parse("1.2-nightly+dev");
            var v2 = SemVersion.Parse("1.2.0-nightly");

            var r = v1.Equals(v2);
            Assert.False(r);
        }

        [Fact]
        public void EqualTest4()
        {
            var v1 = SemVersion.Parse("1.2-nightly");
            var v2 = SemVersion.Parse("1.2.0-nightly2");

            var r = v1.Equals(v2);
            Assert.False(r);
        }

        [Fact]
        public void EqualTest5()
        {
            var v1 = SemVersion.Parse("1.2.1");
            var v2 = SemVersion.Parse("1.2.0");

            var r = v1.Equals(v2);
            Assert.False(r);
        }

        [Fact]
        public void EqualTest6()
        {
            var v1 = SemVersion.Parse("1.4.0");
            var v2 = SemVersion.Parse("1.2.0");

            var r = v1.Equals(v2);
            Assert.False(r);
        }

        [Fact]
        public void EqualByReferenceTest()
        {
            var v1 = SemVersion.Parse("1.2-nightly");

            var r = v1.Equals(v1);
            Assert.True(r);
        }

        [Fact]
        public void CompareTest1()
        {
            var v1 = SemVersion.Parse("1.0.0");
            var v2 = SemVersion.Parse("2.0.0");

            var r = v1.CompareTo(v2);
            Assert.Equal(-1, r);
        }

        [Fact]
        public void CompareTest2()
        {
            var v1 = SemVersion.Parse("1.0.0-beta+dev.123");
            var v2 = SemVersion.Parse("1-beta+dev.123");

            var r = v1.CompareTo(v2);
            Assert.Equal(0, r);
        }

        [Fact]
        public void CompareTest3()
        {
            var v1 = SemVersion.Parse("1.0.0-alpha+dev.123");
            var v2 = SemVersion.Parse("1-beta+dev.123");

            var r = v1.CompareTo(v2);
            Assert.Equal(-1, r);
        }

        [Fact]
        public void CompareTest4()
        {
            var v1 = SemVersion.Parse("1.0.0-alpha");
            var v2 = SemVersion.Parse("1.0.0");

            var r = v1.CompareTo(v2);
            Assert.Equal(-1, r);
        }

        [Fact]
        public void CompareTest5()
        {
            var v1 = SemVersion.Parse("1.0.0");
            var v2 = SemVersion.Parse("1.0.0-alpha");

            var r = v1.CompareTo(v2);
            Assert.Equal(1, r);
        }

        [Fact]
        public void CompareTest6()
        {
            var v1 = SemVersion.Parse("1.0.0");
            var v2 = SemVersion.Parse("1.0.1-alpha");

            var r = v1.CompareTo(v2);
            Assert.Equal(-1, r);
        }

        [Fact]
        public void CompareTest7()
        {
            var v1 = SemVersion.Parse("0.0.1");
            var v2 = SemVersion.Parse("0.0.1+build.12");

            var r = v1.CompareTo(v2);
            Assert.Equal(-1, r);
        }

        [Fact]
        public void CompareTest8()
        {
            var v1 = SemVersion.Parse("0.0.1+build.13");
            var v2 = SemVersion.Parse("0.0.1+build.12.2");

            var r = v1.CompareTo(v2);
            Assert.Equal(1, r);
        }

        [Fact]
        public void CompareTest9()
        {
            var v1 = SemVersion.Parse("0.0.1-13");
            var v2 = SemVersion.Parse("0.0.1-b");

            var r = v1.CompareTo(v2);
            Assert.Equal(-1, r);
        }

        [Fact]
        public void CompareTest10()
        {
            var v1 = SemVersion.Parse("0.0.1+uiui");
            var v2 = SemVersion.Parse("0.0.1+12");

            var r = v1.CompareTo(v2);
            Assert.Equal(1, r);
        }

        [Fact]
        public void CompareTest11()
        {
            var v1 = SemVersion.Parse("0.0.1+bu");
            var v2 = SemVersion.Parse("0.0.1");

            var r = v1.CompareTo(v2);
            Assert.Equal(1, r);
        }

        [Fact]
        public void CompareTest12()
        {
            var v1 = SemVersion.Parse("0.1.1+bu");
            var v2 = SemVersion.Parse("0.2.1");

            var r = v1.CompareTo(v2);
            Assert.Equal(-1, r);
        }

        [Fact]
        public void CompareTest13()
        {
            var v1 = SemVersion.Parse("0.1.1-gamma.12.87");
            var v2 = SemVersion.Parse("0.1.1-gamma.12.88");

            var r = v1.CompareTo(v2);
            Assert.Equal(-1, r);
        }

        [Fact]
        public void CompareTest14()
        {
            var v1 = SemVersion.Parse("0.1.1-gamma.12.87");
            var v2 = SemVersion.Parse("0.1.1-gamma.12.87.1");

            var r = v1.CompareTo(v2);
            Assert.Equal(-1, r);
        }

        [Fact]
        public void CompareTest15()
        {
            var v1 = SemVersion.Parse("0.1.1-gamma.12.87.99");
            var v2 = SemVersion.Parse("0.1.1-gamma.12.87.X");

            var r = v1.CompareTo(v2);
            Assert.Equal(-1, r);
        }

        [Fact]
        public void CompareTest16()
        {
            var v1 = SemVersion.Parse("0.1.1-gamma.12.87");
            var v2 = SemVersion.Parse("0.1.1-gamma.12.87.X");

            var r = v1.CompareTo(v2);
            Assert.Equal(-1, r);
        }

        [Fact]
        public void CompareNullTest()
        {
            var v1 = SemVersion.Parse("0.0.1+bu");
            var r = v1.CompareTo(null);
            Assert.Equal(1, r);
        }

        [Fact]
        public void TestHashCode()
        {
            var v1 = SemVersion.Parse("1.0.0-1+b");
            var v2 = SemVersion.Parse("1.0.0-1+c");

            var h1 = v1.GetHashCode();
            var h2 = v2.GetHashCode();

            Assert.NotEqual(h1, h2);
        }

        [Fact]
        public void TestStringConversion()
        {
            SemVersion v = "1.0.0";
            Assert.Equal(1, v.Major);
        }

        [Fact]
        public void TestUntypedCompareTo()
        {
            var v1 = new SemVersion(1);
            var c = v1.CompareTo((object)v1);

            Assert.Equal(0, c);
        }

        [Fact]
        public void StaticEqualsTest1()
        {
            var v1 = new SemVersion(1, 2, 3);
            var v2 = new SemVersion(1, 2, 3);

            var r = SemVersion.Equals(v1, v2);
            Assert.True(r);
        }

        [Fact]
        public void StaticEqualsTest2()
        {
            var r = SemVersion.Equals(null, null);
            Assert.True(r);
        }

        [Fact]
        public void StaticEqualsTest3()
        {
            var v1 = new SemVersion(1);

            var r = SemVersion.Equals(v1, null);
            Assert.False(r);
        }

        [Fact]
        public void StaticCompareTest1()
        {
            var v1 = new SemVersion(1);
            var v2 = new SemVersion(2);

            var r = SemVersion.Compare(v1, v2);
            Assert.Equal(-1, r);
        }

        [Fact]
        public void StaticCompareTest2()
        {
            var v1 = new SemVersion(1);

            var r = SemVersion.Compare(v1, null);
            Assert.Equal(1, r);
        }

        [Fact]
        public void StaticCompareTest3()
        {
            var v1 = new SemVersion(1);

            var r = SemVersion.Compare(null, v1);
            Assert.Equal(-1, r);
        }

        [Fact]
        public void StaticCompareTest4()
        {
            var r = SemVersion.Compare(null, null);
            Assert.Equal(0, r);
        }

        [Fact]
        public void EqualsOperatorTest()
        {
            var v1 = new SemVersion(1);
            var v2 = new SemVersion(1);

            var r = v1 == v2;
            Assert.True(r);
        }

        [Fact]
        public void UnequalOperatorTest()
        {
            var v1 = new SemVersion(1);
            var v2 = new SemVersion(2);

            var r = v1 != v2;
            Assert.True(r);
        }

        [Fact]
        public void GreaterOperatorTest()
        {
            var v1 = new SemVersion(1);
            var v2 = new SemVersion(2);

            var r = v2 > v1;
            Assert.True(r);
        }

        [Fact]
        public void GreaterOperatorTest2()
        {
            var v1 = new SemVersion(1, 0, 0, "alpha");
            var v2 = new SemVersion(1, 0, 0, "rc");

            var r = v2 > v1;
            Assert.True(r);
        }

        [Fact]
        public void GreaterOperatorTest3()
        {
            var v1 = new SemVersion(1, 0, 0, "-ci.1");
            var v2 = new SemVersion(1, 0, 0, "alpha");

            var r = v2 > v1;
            Assert.True(r);
        }

        [Fact]
        public void GreaterOrEqualOperatorTest1()
        {
            var v1 = new SemVersion(1);
            var v2 = new SemVersion(1);

            var r = v1 >= v2;
            Assert.True(r);
        }

        [Fact]
        public void GreaterOrEqualOperatorTest2()
        {
            var v1 = new SemVersion(2);
            var v2 = new SemVersion(1);

            var r = v1 >= v2;
            Assert.True(r);
        }

        [Fact]
        public void LessOperatorTest()
        {
            var v1 = new SemVersion(1);
            var v2 = new SemVersion(2);

            var r = v1 < v2;
            Assert.True(r);
        }

        [Fact]
        public void LessOperatorTest2()
        {
            var v1 = new SemVersion(1, 0, 0, "alpha");
            var v2 = new SemVersion(1, 0, 0, "rc");

            var r = v1 < v2;
            Assert.True(r);
        }

        [Fact]
        public void LessOperatorTest3()
        {
            var v1 = new SemVersion(1, 0, 0, "-ci.1");
            var v2 = new SemVersion(1, 0, 0, "alpha");

            var r = v1 < v2;
            Assert.True(r);
        }

        [Fact]
        public void LessOrEqualOperatorTest1()
        {
            var v1 = new SemVersion(1);
            var v2 = new SemVersion(1);

            var r = v1 <= v2;
            Assert.True(r);
        }

        [Fact]
        public void LessOrEqualOperatorTest2()
        {
            var v1 = new SemVersion(1);
            var v2 = new SemVersion(2);

            var r = v1 <= v2;
            Assert.True(r);
        }

        [Fact]
        public void TestChangeMajor()
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
        public void TestChangeMinor()
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
        public void TestChangePatch()
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
        public void TestChangePrerelease()
        {
            var v1 = new SemVersion(1, 2, 3, "alpha", "dev");
            var v2 = v1.Change(prerelease: "beta");

            Assert.Equal(1, v2.Major);
            Assert.Equal(2, v2.Minor);
            Assert.Equal(3, v2.Patch);
            Assert.Equal("beta", v2.Prerelease);
            Assert.Equal("dev", v2.Build);
        }

#if !NETSTANDARD
        [Fact]
        public void TestSerialization()
        {
            var semVer = new SemVersion(1, 2, 3, "alpha", "dev");
            SemVersion semVerSerializedDeserialized;
            using (var ms = new MemoryStream())
            {
                var bf = new BinaryFormatter();
                bf.Serialize(ms, semVer);
                ms.Position = 0;
                semVerSerializedDeserialized = (SemVersion) bf.Deserialize(ms);
            }
            Assert.Equal(semVer, semVerSerializedDeserialized);
        }
#endif
    }
}