using System.Collections;
using System.Collections.Generic;
using Semver.Ranges.Comparers.Npm;

namespace Semver.Test.NpmSatisfyTestData
{
    /// <summary>
    /// All values should be parseable ranges and versions
    /// First argument is a range
    /// Second argument is a fully qualified version
    /// Third argument is optional, and is <see cref="NpmParseOptions"/>
    /// Versions should be excluded by the range
    /// </summary>
    public class ExcludeData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            var prOptions = new NpmParseOptions(includePreRelease: true);
            
            yield return new object[] { "1.0.0 - 2.0.0", "2.2.3" };
            yield return new object[] { "1.2.3+asdf - 2.4.3+asdf", "1.2.3-pre.2" };
            yield return new object[] { "1.2.3+asdf - 2.4.3+asdf", "2.4.3-alpha" };
            yield return new object[] { "^1.2.3+build", "2.0.0" };
            yield return new object[] { "^1.2.3+build", "1.2.0" };
            yield return new object[] { "^1.2.3", "1.2.3-pre" };
            yield return new object[] { "^1.2", "1.2.0-pre" };
            yield return new object[] { ">1.2", "1.3.0-beta" };
            yield return new object[] { "<=1.2.3", "1.2.3-beta" };
            yield return new object[] { "^1.2.3", "1.2.3-beta" };
            yield return new object[] { "=0.7.x", "0.7.0-asdf" };
            yield return new object[] { ">=0.7.x", "0.7.0-asdf" };
            yield return new object[] { "<=0.7.x", "0.7.0-asdf" };
            yield return new object[] { "1.0.0", "1.0.1" };
            yield return new object[] { ">=1.0.0", "0.0.0" };
            yield return new object[] { ">=1.0.0", "0.0.1" };
            yield return new object[] { ">=1.0.0", "0.1.0" };
            yield return new object[] { ">1.0.0", "0.0.1" };
            yield return new object[] { ">1.0.0", "0.1.0" };
            yield return new object[] { "<=2.0.0", "3.0.0" };
            yield return new object[] { "<=2.0.0", "2.9999.9999" };
            yield return new object[] { "<=2.0.0", "2.2.9" };
            yield return new object[] { "<2.0.0", "2.9999.9999" };
            yield return new object[] { "<2.0.0", "2.2.9" };
            yield return new object[] { ">=0.1.97", "0.1.93" };
            yield return new object[] { "0.1.20 || 1.2.4", "1.2.3" };
            yield return new object[] { ">=0.2.3 || <0.0.1", "0.0.3" };
            yield return new object[] { ">=0.2.3 || <0.0.1", "0.2.2" };
            yield return new object[] { "2.x.x", "3.1.3" };
            yield return new object[] { "1.2.x", "1.3.3" };
            yield return new object[] { "1.2.x || 2.x", "3.1.3" };
            yield return new object[] { "1.2.x || 2.x", "1.1.3" };
            yield return new object[] { "2.*.*", "1.1.3" };
            yield return new object[] { "2.*.*", "3.1.3" };
            yield return new object[] { "1.2.*", "1.3.3" };
            yield return new object[] { "1.2.* || 2.*", "3.1.3" };
            yield return new object[] { "1.2.* || 2.*", "1.1.3" };
            yield return new object[] { "2", "1.1.2" };
            yield return new object[] { "2.3", "2.4.1" };
            yield return new object[] { "~0.0.1", "0.1.0-alpha" };
            yield return new object[] { "~0.0.1", "0.1.0" };
            yield return new object[] { "~2.4", "2.5.0" }; // >=2.4.0 <2.5.0-0
            yield return new object[] { "~2.4", "2.3.9" };
            yield return new object[] { "~>3.2.1", "3.3.2" }; // >=3.2.1 <3.3.0-0
            yield return new object[] { "~>3.2.1", "3.2.0" }; // >=3.2.1 <3.3.0-0
            yield return new object[] { "~1", "0.2.3" }; // >=1.0.0 <2.0.0-0
            yield return new object[] { "~>1", "2.2.3" };
            yield return new object[] { "~1.0", "1.1.0" }; // >=1.0.0 <1.1.0-0
            yield return new object[] { "<1", "1.0.0" };
            yield return new object[] { ">=1.2", "1.1.1" };
            yield return new object[] { "~v0.5.4-beta", "0.5.4-alpha" };
            yield return new object[] { "=0.7.x", "0.8.2" };
            yield return new object[] { ">=0.7.x", "0.6.2" };
            yield return new object[] { "<0.7.x", "0.7.2" };
            yield return new object[] { "<1.2.3", "1.2.3-beta" };
            yield return new object[] { "=1.2.3", "1.2.3-beta" };
            yield return new object[] { ">1.2", "1.2.8" };
            yield return new object[] { "^0.0.1", "0.0.2-alpha" };
            yield return new object[] { "^0.0.1", "0.0.2" };
            yield return new object[] { "^1.2.3", "2.0.0-alpha" };
            yield return new object[] { "^1.2.3", "1.2.2" };
            yield return new object[] { "^1.2", "1.1.9" };

            yield return new object[] { "2.x", "3.0.0-pre.0", prOptions };
            yield return new object[] { "^1.0.0", "1.0.0-rc1", prOptions };
            yield return new object[] { "^1.0.0", "2.0.0-rc1", prOptions };
            yield return new object[] { "^1.2.3-rc2", "2.0.0", prOptions };
            yield return new object[] { "^1.0.0", "2.0.0-rc1", prOptions };
            yield return new object[] { "^1.0.0", "2.0.0-rc1" };

            yield return new object[] { "1 - 2", "3.0.0-pre", prOptions };
            yield return new object[] { "1 - 2", "2.0.0-pre" };
            yield return new object[] { "1 - 2", "1.0.0-pre" };
            yield return new object[] { "1.0 - 2", "1.0.0-pre" };

            yield return new object[] { "1.1.x", "1.0.0-a" };
            yield return new object[] { "1.1.x", "1.1.0-a" };
            yield return new object[] { "1.1.x", "1.2.0-a" };
            yield return new object[] { "1.1.x", "1.2.0-a", prOptions };
            yield return new object[] { "1.1.x", "1.0.0-a", prOptions };
            yield return new object[] { "1.x", "1.0.0-a" };
            yield return new object[] { "1.x", "1.1.0-a" };
            yield return new object[] { "1.x", "1.2.0-a" };
            yield return new object[] { "1.x", "0.0.0-a", prOptions };
            yield return new object[] { "1.x", "2.0.0-a", prOptions };

            yield return new object[] { ">=1.0.0 <1.1.0", "1.1.0" };
            yield return new object[] { ">=1.0.0 <1.1.0", "1.1.0", prOptions };
            yield return new object[] { ">=1.0.0 <1.1.0", "1.1.0-pre" };
            yield return new object[] { ">=1.0.0 <1.1.0-pre", "1.1.0-pre" };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
