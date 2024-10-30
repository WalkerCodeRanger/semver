using System;
using System.Collections.Generic;
using System.Linq;
using Semver.Test.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Semver.Test;

/// <summary>
/// These tests verify the syntax and output of the examples in the README.md file.
/// </summary>
public class ReadmeTests
{
    private readonly ITestOutputHelper console;

    public ReadmeTests(ITestOutputHelper console)
    {
        this.console = console;
    }

    [Fact]
    public void ReadmeRuns()
    {
        // Parsing
        var version = SemVersion.Parse("1.1.0-rc.1+e471d15", SemVersionStyles.Strict);

        // Constructing
        var v1 = new SemVersion(1, 0);
        var vNextRc = SemVersion.ParsedFrom(1, 1, 0, "rc.1");

        // Comparing
        console.WriteLine("");
        console.WriteLine("# Comparing");
        if (version.ComparePrecedenceTo(vNextRc) == 0)
            console.WriteLine($"{version} has the same precedence as {vNextRc}");

        if (version.CompareSortOrderTo(vNextRc) > 0)
            console.WriteLine($"{version} sorts after {vNextRc}");

        // Sorting and Enumerable Max/Min
        console.WriteLine("");
        console.WriteLine("# Sorting and Enumerable Max/Min");
        var examples = new List<Example> { Example.Example1, Example.Example2, Example.Example3, Example.Example4 };
        // Put in sort order
        var sorted = examples.OrderBy(e => e.Version, SemVersion.SortOrderComparer);
        // Order by precedence, then by release date
        var ordered = examples.OrderBy(e => e.Version, SemVersion.PrecedenceComparer)
                              .ThenBy(e => e.Released);

        var versions = new List<SemVersion> { vNextRc, v1 };
        var max = versions.Max(SemVersion.SortOrderComparer);
        console.WriteLine($"Max version is {max}");

        // Manipulating
        console.WriteLine("");
        console.WriteLine("# Manipulating");
        console.WriteLine($"Current: {version}");
        if (version.IsPrerelease)
        {
            console.WriteLine($"Prerelease: {version.Prerelease}");
            console.WriteLine($"Next release version is: {version.WithoutPrereleaseOrMetadata()}");
        }

        // Version Ranges
        console.WriteLine("");
        console.WriteLine("# Version Ranges");
        var range = SemVersionRange.Parse("^1.0.0");
        var prereleaseRange = SemVersionRange.ParseNpm("^1.0.0", includeAllPrerelease: true);
        console.WriteLine($"Range: {range}");
        console.WriteLine($"Prerelease range: {prereleaseRange}");
        console.WriteLine($"Range includes version {version}: {range.Contains(version)}");
        console.WriteLine($"Prerelease range includes version {version}: {prereleaseRange.Contains(version)}");

        // Alternative: another way to call SemVersionRange.Contains(version)
        Assert.False(version.Satisfies(range));

        // Alternative: slower because it parses the range on every call
        Assert.True(version.SatisfiesNpm("^1.0.0", includeAllPrerelease: true));
    }

    private class Example(SemVersion version, DateTime released)
    {
        public static readonly Example Example1 = new Example(SemVersion.Parse("1.2.3+Foo"), new(2024, 10, 1));
        public static readonly Example Example2 = new Example(SemVersion.Parse("1.2.3+Bar"), new(2024, 10, 15));
        public static readonly Example Example3 = new Example(SemVersion.Parse("1.2.3+Baz"), new(2024, 10, 30));
        public static readonly Example Example4 = new Example(SemVersion.Parse("1.2.3-pre"), new(2024, 10, 31));

        public SemVersion Version { get; } = version;
        public DateTime Released { get; } = released;
    }
}
