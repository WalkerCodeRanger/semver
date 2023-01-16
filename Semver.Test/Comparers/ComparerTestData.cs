using System.Collections.Generic;
using Semver.Test.Helpers;
using Semver.Utility;

namespace Semver.Test.Comparers
{
    /// <summary>
    /// Data for testing any comparison or equality related functionality of <see cref="SemVersion"/>.
    /// This includes both standard comparison and precedence comparison. It also includes
    /// <see cref="SemVersion.GetHashCode()"/> because this is connected to equality.
    ///
    /// Because it is possible to construct invalid versions, the comparison
    /// tests must be based off constructing <see cref="SemVersion"/> rather than just
    /// using version strings. The approach used is to work from a list of versions
    /// in their correct order and then compare versions within the list. To
    /// avoid issues with xUnit serialization of <see cref="SemVersion"/>, this
    /// is done within the test rather than using theory data.
    /// </summary>
    public static class ComparerTestData
    {
        // TODO this could use string now
        public static readonly IReadOnlyList<SemVersion> VersionsInSortOrder = new List<SemVersion>()
        {
            SemVersion.ParsedFrom(0),
            SemVersion.ParsedFrom(0, 0, 1, "13"),
            SemVersion.ParsedFrom(0, 0, 1, "b"),
            SemVersion.ParsedFrom(0, 0, 1, "gamma.12.87"),
            SemVersion.ParsedFrom(0, 0, 1, "gamma.12.87.1"),
            SemVersion.ParsedFrom(0, 0, 1, "gamma.12.87.99"),
            SemVersion.ParsedFrom(0, 0, 1, "gamma.12.87.-"),
            SemVersion.ParsedFrom(0, 0, 1, "gamma.12.87.X"),
            SemVersion.ParsedFrom(0, 0, 1, "gamma.12.88"),
            SemVersion.ParsedFrom(0, 0, 1),
            SemVersion.ParsedFrom(0, 0, 1, "", "12"),
            SemVersion.ParsedFrom(0, 0, 1, "", "b"),
            SemVersion.ParsedFrom(0, 0, 1, "", "bu"),
            SemVersion.ParsedFrom(0, 0, 1, "", "build.-"),
            SemVersion.ParsedFrom(0, 0, 1, "", "build.12"),
            SemVersion.ParsedFrom(0, 0, 1, "", "build.12.2"),
            SemVersion.ParsedFrom(0, 0, 1, "", "build.13"),
            SemVersion.ParsedFrom(0, 0, 1, "", "uiui"),
            SemVersion.ParsedFrom(0, 1, 1),
            SemVersion.ParsedFrom(0, 2, 1),
            SemVersion.ParsedFrom(1, 0, 0, "alpha"),
            SemVersion.ParsedFrom(1, 0, 0, "alpha", "dev.123"),
            SemVersion.ParsedFrom(1, 0, 0, "alpha.1"),
            SemVersion.ParsedFrom(1, 0, 0, "alpha.-"),
            SemVersion.ParsedFrom(1, 0, 0, "alpha.beta"),
            SemVersion.ParsedFrom(1, 0, 0, "beta"),
            SemVersion.ParsedFrom(1, 0, 0, "beta", "dev.123"),
            SemVersion.ParsedFrom(1, 0, 0, "beta.2"),
            SemVersion.ParsedFrom(1, 0, 0, "beta.11"),
            SemVersion.ParsedFrom(1, 0, 0, "rc.1"),
            SemVersion.ParsedFrom(1),
            SemVersion.ParsedFrom(1, 0, 0, "", "CA6B10F"),
            SemVersion.ParsedFrom(1, 0, 10, "alpha"),
            SemVersion.ParsedFrom(1, 2, 0, "alpha", "dev"),
            SemVersion.ParsedFrom(1, 2, 0, "nightly"),
            SemVersion.ParsedFrom(1, 2, 0, "nightly", "dev"),
            SemVersion.ParsedFrom(1, 2, 0, "nightly2"),
            SemVersion.ParsedFrom(1, 2),
            SemVersion.ParsedFrom(1, 2, 0, "", "nightly"),
            SemVersion.ParsedFrom(1, 2, 1, "0"),
            SemVersion.ParsedFrom(1, 2, 1, "1"),
            SemVersion.ParsedFrom(1, 2, 1, "2"),
            SemVersion.ParsedFrom(1, 2, 1, "10"), // Sorts numeric after 2
            SemVersion.ParsedFrom(1, 2, 1, "99"),
            SemVersion.ParsedFrom(1, 2, 1, "-"),
            SemVersion.ParsedFrom(1, 2, 1, "-1"), // Sorts alphanumeric instead of numeric
            SemVersion.ParsedFrom(1, 2, 1, "-a"),
            SemVersion.ParsedFrom(1, 2, 1, "0A"),
            SemVersion.ParsedFrom(1, 2, 1, "A"),
            SemVersion.ParsedFrom(1, 2, 1, "a"),
            SemVersion.ParsedFrom(1, 2, 1),
            SemVersion.ParsedFrom(1, 2, 3, "1"),
            SemVersion.ParsedFrom(1, 2, 3, "a.1"),
            SemVersion.ParsedFrom(1, 2, 3, "", "01"),
            SemVersion.ParsedFrom(1, 2, 3, "", "1"),
            SemVersion.ParsedFrom(1, 2, 3, "", "a.000001"),
            SemVersion.ParsedFrom(1, 2, 3, "", "a.01"),
            SemVersion.ParsedFrom(1, 2, 3, "", "a.1"),
            SemVersion.ParsedFrom(1, 4),
            SemVersion.ParsedFrom(2),
            SemVersion.ParsedFrom(2, 1),
            SemVersion.ParsedFrom(2, 1, 1),
        }.AsReadOnly();

        public static readonly IReadOnlyList<(SemVersion, SemVersion)> VersionPairs
            = VersionsInSortOrder.AllPairs().ToReadOnlyList();
    }
}
