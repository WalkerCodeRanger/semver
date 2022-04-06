using System.Collections;
using System.Collections.Generic;
using Semver.Ranges.Comparers.Npm;

namespace Semver.Test.NpmSatisfyTestData
{
    public class ParseData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { "1.0.0 - 2.0.0", ">=1.0.0 <=2.0.0" };
            yield return new object[] { "1.0.0 - 2.0.0", ">=1.0.0-0 <2.0.1-0", new NpmParseOptions(includePreRelease: true) };
            yield return new object[] { "1 - 2", ">=1.0.0 <3.0.0-0" };
            yield return new object[] { "1 - 2", ">=1.0.0-0 <3.0.0-0", new NpmParseOptions(includePreRelease: true) };
            yield return new object[] { "1.0 - 2.0", ">=1.0.0 <2.1.0-0" };
            yield return new object[] { "1.0 - 2.0", ">=1.0.0-0 <2.1.0-0", new NpmParseOptions(includePreRelease: true) };
            yield return new object[] { "1.0.0", "1.0.0" };
            yield return new object[] { ">=*", "*" };
            yield return new object[] { "", "*" };
            yield return new object[] { "*", "*" };
            yield return new object[] { "*", "*" };
            yield return new object[] { ">=1.0.0", ">=1.0.0" };
            yield return new object[] { ">1.0.0", ">1.0.0" };
            yield return new object[] { "<=2.0.0", "<=2.0.0" };
            yield return new object[] { "<=2", "<3.0.0-0" };
            yield return new object[] { "<=2.0", "<2.1.0-0" };
            yield return new object[] { "1", ">=1.0.0 <2.0.0-0" };
            yield return new object[] { "<=2.0.0", "<=2.0.0" };
            yield return new object[] { "<=2.0.0", "<=2.0.0" };
            yield return new object[] { "<2.0.0", "<2.0.0" };
            yield return new object[] { "<2.0.0", "<2.0.0" };
            yield return new object[] { ">= 1.0.0", ">=1.0.0" };
            yield return new object[] { ">=  1.0.0", ">=1.0.0" };
            yield return new object[] { ">=   1.0.0", ">=1.0.0" };
            yield return new object[] { "> 1.0.0", ">1.0.0" };
            yield return new object[] { ">  1.0.0", ">1.0.0" };
            yield return new object[] { "<=   2.0.0", "<=2.0.0" };
            yield return new object[] { "<= 2.0.0", "<=2.0.0" };
            yield return new object[] { "<=  2.0.0", "<=2.0.0" };
            yield return new object[] { "<    2.0.0", "<2.0.0" };
            yield return new object[] { "<\t2.0.0", "<2.0.0" };
            yield return new object[] { ">=0.1.97", ">=0.1.97" };
            yield return new object[] { ">=0.1.97", ">=0.1.97" };
            yield return new object[] { "0.1.20 || 1.2.4", "0.1.20 || 1.2.4" };
            yield return new object[] { ">=0.2.3 || <0.0.1", ">=0.2.3 || <0.0.1" };
            yield return new object[] { ">=0.2.3 || <0.0.1", ">=0.2.3 || <0.0.1" };
            yield return new object[] { ">=0.2.3 || <0.0.1", ">=0.2.3 || <0.0.1" };
            yield return new object[] { "||", "*" };
            yield return new object[] { "2.x.x", ">=2.0.0 <3.0.0-0" };
            yield return new object[] { "1.2.x", ">=1.2.0 <1.3.0-0" };
            yield return new object[] { "1.2.x || 2.x", ">=1.2.0 <1.3.0-0 || >=2.0.0 <3.0.0-0" };
            yield return new object[] { "1.2.x || 2.x", ">=1.2.0 <1.3.0-0 || >=2.0.0 <3.0.0-0" };
            yield return new object[] { "x", "*" };
            yield return new object[] { "2.*.*", ">=2.0.0 <3.0.0-0" };
            yield return new object[] { "1.2.*", ">=1.2.0 <1.3.0-0" };
            yield return new object[] { "1.2.* || 2.*", ">=1.2.0 <1.3.0-0 || >=2.0.0 <3.0.0-0" };
            yield return new object[] { "*", "*" };
            yield return new object[] { "2", ">=2.0.0 <3.0.0-0" };
            yield return new object[] { "2.3", ">=2.3.0 <2.4.0-0" };
            yield return new object[] { "~2.4", ">=2.4.0 <2.5.0-0" };
            yield return new object[] { "~2.4", ">=2.4.0 <2.5.0-0" };
            yield return new object[] { "~>3.2.1", ">=3.2.1 <3.3.0-0" };
            yield return new object[] { "~1", ">=1.0.0 <2.0.0-0" };
            yield return new object[] { "~>1", ">=1.0.0 <2.0.0-0" };
            yield return new object[] { "~> 1", ">=1.0.0 <2.0.0-0" };
            yield return new object[] { "~1.0", ">=1.0.0 <1.1.0-0" };
            yield return new object[] { "~ 1.0", ">=1.0.0 <1.1.0-0" };
            yield return new object[] { "^0", "<1.0.0-0" };
            yield return new object[] { "^ 1", ">=1.0.0 <2.0.0-0" };
            yield return new object[] { "^0.1", ">=0.1.0 <0.2.0-0" };
            yield return new object[] { "^1.0", ">=1.0.0 <2.0.0-0" };
            yield return new object[] { "^1.2", ">=1.2.0 <2.0.0-0" };
            yield return new object[] { "^0.0.1", ">=0.0.1 <0.0.2-0" };
            yield return new object[] { "^0.0.1-beta", ">=0.0.1-beta <0.0.2-0" };
            yield return new object[] { "^0.1.2", ">=0.1.2 <0.2.0-0" };
            yield return new object[] { "^1.2.3", ">=1.2.3 <2.0.0-0" };
            yield return new object[] { "^1.2.3-beta.4", ">=1.2.3-beta.4 <2.0.0-0" };
            yield return new object[] { "<1", "<1.0.0-0" };
            yield return new object[] { "< 1", "<1.0.0-0" };
            yield return new object[] { ">=1", ">=1.0.0" };
            yield return new object[] { ">= 1", ">=1.0.0" };
            yield return new object[] { "<1.2", "<1.2.0-0" };
            yield return new object[] { "< 1.2", "<1.2.0-0" };
            yield return new object[] { ">01.02.03", ">1.2.3" };
            yield return new object[] { "~1.2.3-beta", ">=1.2.3-beta <1.3.0-0" };
            yield return new object[] { "^ 1.2 ^ 1", ">=1.2.0 <2.0.0-0 >=1.0.0" };
            yield return new object[] { "1.2 - 3.4.5", ">=1.2.0 <=3.4.5" };
            yield return new object[] { "1.2.3 - 3.4", ">=1.2.3 <3.5.0-0" };
            yield return new object[] { "1.2 - 3.4", ">=1.2.0 <3.5.0-0" };
            yield return new object[] { ">1", ">=2.0.0" };
            yield return new object[] { ">1.2", ">=1.3.0" };
            yield return new object[] { ">X", "<0.0.0-0" };
            yield return new object[] { "<X", "<0.0.0-0" };
            yield return new object[] { "<x <* || >* 2.x", "<0.0.0-0" };
            yield return new object[] { ">x 2.x || * || <x", "*" };
            yield return new object[] { ">=09090", ">=9090.0.0" };
            yield return new object[] { ">=09090", ">=9090.0.0-0", new NpmParseOptions(includePreRelease: true) };
            yield return new object[] { ">09090", ">=9091.0.0" };
            yield return new object[] { ">09090", ">=9091.0.0-0", new NpmParseOptions(includePreRelease: true) };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
