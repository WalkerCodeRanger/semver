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
        console.WriteLine("# Comparing");
        if (version.ComparePrecedenceTo(vNextRc) == 0)
            console.WriteLine($"{version} has the same precedence as {vNextRc}");

        if (version.CompareSortOrderTo(vNextRc) > 0)
            console.WriteLine($"{version} sorts after {vNextRc}");

        // Manipulating
        console.WriteLine("# Manipulating");
        console.WriteLine($"Current: {version}");
        if (version.IsPrerelease)
        {
            console.WriteLine($"Prerelease: {version.Prerelease}");
            console.WriteLine($"Next release version is: {version.WithoutPrereleaseOrMetadata()}");
        }

        // Version Ranges
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
}
