using System.Collections;
using System.Collections.Generic;
using Semver.Ranges.Comparers.Npm;

namespace Semver.Test.NpmSatisfyTestData
{
    /// <summary>
    /// All values should be parseable ranges
    /// </summary>
    public class IncludeData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { "1.0.0 - 2.0.0", "1.2.3" };
            yield return new object[] { "1.0.0 - 2.0.0", "1.2.3" };
            yield return new object[] { "^1.2.3+build", "1.2.3" };
            yield return new object[] { "^1.2.3+build", "1.3.0" };
            yield return new object[] { "1.2.3-pre+asdf - 2.4.3-pre+asdf", "1.2.3" };
            yield return new object[] { "1.2.3-pre+asdf - 2.4.3-pre+asdf", "1.2.3-pre.2", new NpmParseOptions(includePreRelease: true) };
            yield return new object[] { "1.2.3-pre+asdf - 2.4.3-pre+asdf", "2.4.3-alpha", new NpmParseOptions(includePreRelease: true) };
            yield return new object[] { "1.2.3+asdf - 2.4.3+asdf", "1.2.3" };
            yield return new object[] { "1.0.0", "1.0.0" };
            yield return new object[] { ">=*", "0.2.4" };
            yield return new object[] { "", "1.0.0" };
            yield return new object[] { "*", "1.2.3" };
            yield return new object[] { "*", "1.2.3" };
            yield return new object[] { ">=1.0.0", "1.0.0" };
            yield return new object[] { ">=1.0.0", "1.0.1" };
            yield return new object[] { ">=1.0.0", "1.1.0" };
            yield return new object[] { ">1.0.0", "1.0.1" };
            yield return new object[] { ">1.0.0", "1.1.0" };
            yield return new object[] { "<=2.0.0", "2.0.0" };
            yield return new object[] { "<=2.0.0", "1.9999.9999" };
            yield return new object[] { "<=2.0.0", "0.2.9" };
            yield return new object[] { "<2.0.0", "1.9999.9999" };
            yield return new object[] { "<2.0.0", "0.2.9" };
            yield return new object[] { ">= 1.0.0", "1.0.0" };
            yield return new object[] { ">=  1.0.0", "1.0.1" };
            yield return new object[] { ">=   1.0.0", "1.1.0" };
            yield return new object[] { "> 1.0.0", "1.0.1" };
            yield return new object[] { ">  1.0.0", "1.1.0" };
            yield return new object[] { "<=   2.0.0", "2.0.0" };
            yield return new object[] { "<= 2.0.0", "1.9999.9999" };
            yield return new object[] { "<=  2.0.0", "0.2.9" };
            yield return new object[] { "<    2.0.0", "1.9999.9999" };
            yield return new object[] { "<\t2.0.0", "0.2.9" };
            yield return new object[] { ">=0.1.97", "0.1.97" };
            yield return new object[] { ">=0.1.97", "0.1.97" };
            yield return new object[] { "0.1.20 || 1.2.4", "1.2.4" };
            yield return new object[] { ">=0.2.3 || <0.0.1", "0.0.0" };
            yield return new object[] { ">=0.2.3 || <0.0.1", "0.2.3" };
            yield return new object[] { ">=0.2.3 || <0.0.1", "0.2.4" };
            yield return new object[] { "||", "1.3.4" };
            yield return new object[] { "2.x.x", "2.1.3" };
            yield return new object[] { "1.2.x", "1.2.3" };
            yield return new object[] { "1.2.x || 2.x", "2.1.3" };
            yield return new object[] { "1.2.x || 2.x", "1.2.3" };
            yield return new object[] { "x", "1.2.3" };
            yield return new object[] { "2.*.*", "2.1.3" };
            yield return new object[] { "1.2.*", "1.2.3" };
            yield return new object[] { "1.2.* || 2.*", "2.1.3" };
            yield return new object[] { "1.2.* || 2.*", "1.2.3" };
            yield return new object[] { "*", "1.2.3" };
            yield return new object[] { "2", "2.1.2" };
            yield return new object[] { "2.3", "2.3.1" };
            yield return new object[] { "~0.0.1", "0.0.1" };
            yield return new object[] { "~0.0.1", "0.0.2" };
            yield return new object[] { "~2", "2.9.9" }; // >=2.0.0 <3.0.0
            yield return new object[] { "~2.4", "2.4.0" }; // >=2.4.0 <2.5.0
            yield return new object[] { "~2.4", "2.4.5" };
            yield return new object[] { "~1", "1.2.3" }; // >=1.0.0 <2.0.0
            yield return new object[] { "~1.0", "1.0.2" }; // >=1.0.0 <1.1.0,
            yield return new object[] { "~ 1.0", "1.0.2" };
            yield return new object[] { "~ 1.0.3", "1.0.12" };
            yield return new object[] { ">=1", "1.0.0" };
            yield return new object[] { ">= 1", "1.0.0" };
            yield return new object[] { "<1.2", "1.1.1" };
            yield return new object[] { "< 1.2", "1.1.1" };
            yield return new object[] { "~0.5.4-pre", "0.5.5" };
            yield return new object[] { "~0.5.4-pre", "0.5.4" };
            yield return new object[] { "=0.7.x", "0.7.2" };
            yield return new object[] { "<=0.7.x", "0.7.2" };
            yield return new object[] { ">=0.7.x", "0.7.2" };
            yield return new object[] { "<=0.7.x", "0.6.2" };
            yield return new object[] { "~1.2.1 >=1.2.3", "1.2.3" };
            yield return new object[] { "~1.2.1 =1.2.3", "1.2.3" };
            yield return new object[] { "~1.2.1 1.2.3", "1.2.3" };
            yield return new object[] { "~1.2.1 >=1.2.3 1.2.3", "1.2.3" };
            yield return new object[] { "~1.2.1 1.2.3 >=1.2.3", "1.2.3" };
            yield return new object[] { "~1.2.1 1.2.3", "1.2.3" };
            yield return new object[] { ">=1.2.1 1.2.3", "1.2.3" };
            yield return new object[] { "1.2.3 >=1.2.1", "1.2.3" };
            yield return new object[] { ">=1.2.3 >=1.2.1", "1.2.3" };
            yield return new object[] { ">=1.2.1 >=1.2.3", "1.2.3" };
            yield return new object[] { ">=1.2", "1.2.8" };
            yield return new object[] { "^1.2.3", "1.8.1" };
            yield return new object[] { "^0.1.2", "0.1.2" };
            yield return new object[] { "^0.1", "0.1.2" };
            yield return new object[] { "^0.0.1", "0.0.1" };
            yield return new object[] { "^1.2", "1.4.2" };
            yield return new object[] { "^1.2 ^1", "1.4.2" };
            yield return new object[] { "^1.2.3-alpha", "1.2.3-pre" };
            yield return new object[] { "^1.2.0-alpha", "1.2.0-pre" };
            yield return new object[] { "^0.0.1-alpha", "0.0.1-beta" };
            yield return new object[] { "^0.0.1-alpha", "0.0.1" };
            yield return new object[] { "^0.1.1-alpha", "0.1.1-beta" };
            yield return new object[] { "^x", "1.2.3" };
            yield return new object[] { "x - 1.0.0", "0.9.7" };
            yield return new object[] { "x - 1.x", "0.9.7" };
            yield return new object[] { "1.0.0 - x", "1.9.7" };
            yield return new object[] { "1.x - x", "1.9.7" };
            yield return new object[] { "<=7.x", "7.9.9" };
            yield return new object[] { "2.x", "2.0.0-pre.0", new NpmParseOptions(includePreRelease: true) };
            yield return new object[] { "2.x", "2.1.0-pre.0", new NpmParseOptions(includePreRelease: true) };
            yield return new object[] { "1.1.x", "1.1.0-a", new NpmParseOptions(includePreRelease: true) };
            yield return new object[] { "1.1.x", "1.1.1-a", new NpmParseOptions(includePreRelease: true) };
            yield return new object[] { "*", "1.0.0-rc1", new NpmParseOptions(includePreRelease: true) };
            yield return new object[] { "^1.0.0-0", "1.0.1-rc1", new NpmParseOptions(includePreRelease: true) };
            yield return new object[] { "^1.0.0-rc2", "1.0.1-rc1", new NpmParseOptions(includePreRelease: true) };
            yield return new object[] { "^1.0.0", "1.0.1-rc1", new NpmParseOptions(includePreRelease: true) };
            yield return new object[] { "^1.0.0", "1.1.0-rc1", new NpmParseOptions(includePreRelease: true) };
            yield return new object[] { "1 - 2", "2.0.0-pre", new NpmParseOptions(includePreRelease: true) };
            yield return new object[] { "1 - 2", "1.0.0-pre", new NpmParseOptions(includePreRelease: true) };
            yield return new object[] { "1.0 - 2", "1.0.0-pre", new NpmParseOptions(includePreRelease: true) };
            yield return new object[] { "=0.7.x", "0.7.0-asdf", new NpmParseOptions(includePreRelease: true) };
            yield return new object[] { ">=0.7.x", "0.7.0-asdf", new NpmParseOptions(includePreRelease: true) };
            yield return new object[] { "<=0.7.x", "0.7.0-asdf", new NpmParseOptions(includePreRelease: true) };
            yield return new object[] { ">=1.0.0 <=1.1.0", "1.1.0-pre", new NpmParseOptions(includePreRelease: true) };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
