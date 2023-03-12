using System.Collections.Generic;
using System.Linq;
using Semver.Test.Helpers;
using Semver.Utility;
using Xunit;

namespace Semver.Test.Comparers
{
    /// <summary>
    /// <para>Data for testing any comparison or equality related functionality of
    /// <see cref="SemVersion"/>. This includes both standard comparison and precedence comparison.
    /// It also includes <see cref="SemVersion.GetHashCode()"/> because this is connected to
    /// equality.</para>
    ///
    /// <para>The approach used is to work from a list of versions in their correct order and then
    /// compare versions within the list.</para>
    /// </summary>
    public static class ComparerTestData
    {
        public static readonly TheoryData<string> VersionsInSortOrder = new TheoryData<string>()
        {
            "0.0.0",
            "0.0.1-13",
            "0.0.1-b",
            "0.0.1-gamma.12.87",
            "0.0.1-gamma.12.87.1",
            "0.0.1-gamma.12.87.99",
            "0.0.1-gamma.12.87.-",
            "0.0.1-gamma.12.87.X",
            "0.0.1-gamma.12.88",
            "0.0.1",
            "0.0.1+12",
            "0.0.1+b",
            "0.0.1+bu",
            "0.0.1+build.-",
            "0.0.1+build.12",
            "0.0.1+build.12.2",
            "0.0.1+build.13",
            "0.0.1+uiui",
            "0.1.1",
            "0.2.1",
            "1.0.0-alpha",
            "1.0.0-alpha+dev.123",
            "1.0.0-alpha.1",
            "1.0.0-alpha.-",
            "1.0.0-alpha.beta",
            "1.0.0-beta",
            "1.0.0-beta+dev.123",
            "1.0.0-beta.2",
            "1.0.0-beta.11",
            "1.0.0-rc.1",
            "1.0.0",
            "1.0.0+CA6B10F",
            "1.0.10-alpha",
            "1.2.0-alpha+dev",
            "1.2.0-nightly",
            "1.2.0-nightly+dev",
            "1.2.0-nightly2",
            "1.2.0",
            "1.2.0+nightly",
            "1.2.1-0",
            "1.2.1-1",
            "1.2.1-2",
            "1.2.1-10", // Sorts numeric after 2
            "1.2.1-99",
            "1.2.1--",
            "1.2.1--1", // Sorts alphanumeric instead of numeric
            "1.2.1--a",
            "1.2.1-0A",
            "1.2.1-A",
            "1.2.1-a",
            "1.2.1",
            "1.2.3-1",
            "1.2.3-a.1",
            "1.2.3+01",
            "1.2.3+1",
            "1.2.3+a.000001",
            "1.2.3+a.01",
            "1.2.3+a.1",
            "1.4.0",
            "2.0.0",
            "2.1.0",
            "2.1.1",
        };
        
        public static readonly TheoryData<string,string> VersionPairs = VersionsInSortOrder.AllPairs();
    }
}
