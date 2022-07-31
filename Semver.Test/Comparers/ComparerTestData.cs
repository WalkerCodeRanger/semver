using System.Collections.Generic;
using Semver.Test.Builders;
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
        public static readonly IReadOnlyList<SemVersion> VersionsInSortOrder = new List<SemVersion>()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            new SemVersion(-2),
            new SemVersion(-1, -1),
            new SemVersion(-1),
            new SemVersion(0, -1),
            new SemVersion(0, 0, -1),
            new SemVersion(0),
            new SemVersion(0, 0, 1, "13"),
            new SemVersion(0, 0, 1, "."),
            new SemVersion(0, 0, 1, ".."),
            new SemVersion(0, 0, 1, ".a"),
            new SemVersion(0, 0, 1, "b"),
            new SemVersion(0, 0, 1, "gamma.12.87"),
            new SemVersion(0, 0, 1, "gamma.12.87.1"),
            new SemVersion(0, 0, 1, "gamma.12.87.99"),
            new SemVersion(0, 0, 1, "gamma.12.87.-"),
            new SemVersion(0, 0, 1, "gamma.12.87.X"),
            new SemVersion(0, 0, 1, "gamma.12.88"),
            new SemVersion(0, 0, 1),
            new SemVersion(0, 0, 1, "", "."),
            new SemVersion(0, 0, 1, "", ".."),
            new SemVersion(0, 0, 1, "", ".a"),
            new SemVersion(0, 0, 1, "", "12"),
            new SemVersion(0, 0, 1, "", "b"),
            new SemVersion(0, 0, 1, "", "bu"),
            new SemVersion(0, 0, 1, "", "build.-"),
            new SemVersion(0, 0, 1, "", "build.12"),
            new SemVersion(0, 0, 1, "", "build.12.2"),
            new SemVersion(0, 0, 1, "", "build.13"),
            new SemVersion(0, 0, 1, "", "uiui"),
            new SemVersion(0, 1, 1),
            new SemVersion(0, 2, 1),
            new SemVersion(1, 0, 0, "alpha"),
            new SemVersion(1, 0, 0, "alpha", "dev.123"),
            new SemVersion(1, 0, 0, "alpha", "😞"),
            new SemVersion(1, 0, 0, "alpha.1"),
            new SemVersion(1, 0, 0, "alpha.-"),
            new SemVersion(1, 0, 0, "alpha.beta"),
            new SemVersion(1, 0, 0, "beta"),
            new SemVersion(1, 0, 0, "beta", "dev.123"),
            new SemVersion(1, 0, 0, "beta.2"),
            new SemVersion(1, 0, 0, "beta.11"),
            new SemVersion(1, 0, 0, "rc.1"),
            new SemVersion(1, 0, 0, "😞"),
            new SemVersion(1),
            new SemVersion(1, 0, 0, "", "CA6B10F"),
            new SemVersion(1, 0, 10, "alpha"),
            new SemVersion(1, 2, 0, "alpha", "dev"),
            new SemVersion(1, 2, 0, "nightly"),
            new SemVersion(1, 2, 0, "nightly", "dev"),
            new SemVersion(1, 2, 0, "nightly2"),
            new SemVersion(1, 2),
            new SemVersion(1, 2, 0, "", "nightly"),
            new SemVersion(1, 2, 1, "0"),
            new SemVersion(1, 2, 1, "1"),
            new SemVersion(1, 2, 1, "2"),
            new SemVersion(1, 2, 1, "10"), // Sorts numeric after 2
            new SemVersion(1, 2, 1, "99"),
            new SemVersion(1, 2, 1, "-"),
            new SemVersion(1, 2, 1, "-1"), // Sorts alphanumeric instead of numeric
            new SemVersion(1, 2, 1, "-a"),
            new SemVersion(1, 2, 1, "0A"),
            new SemVersion(1, 2, 1, "A"),
            new SemVersion(1, 2, 1, "a"),
            new SemVersion(1, 2, 1),
            new SemVersion(1, 2, 3, "01"),
            new SemVersion(1, 2, 3, "1"),
            new SemVersion(1, 2, 3, "a.000001"),
            new SemVersion(1, 2, 3, "a.01"),
            new SemVersion(1, 2, 3, "a.1"),
            new SemVersion(1, 2, 3, "", "01"),
            new SemVersion(1, 2, 3, "", "1"),
            new SemVersion(1, 2, 3, "", "a.000001"),
            new SemVersion(1, 2, 3, "", "a.01"),
            new SemVersion(1, 2, 3, "", "a.1"),
            new SemVersion(1, 4),
            new SemVersion(2),
            new SemVersion(2, 1),
            new SemVersion(2, 1, 1),
#pragma warning restore CS0618 // Type or member is obsolete
        }.AsReadOnly();

        public static readonly IReadOnlyList<(SemVersion, SemVersion)> VersionPairs
            = VersionsInSortOrder.AllPairs().ToReadOnlyList();
    }
}
