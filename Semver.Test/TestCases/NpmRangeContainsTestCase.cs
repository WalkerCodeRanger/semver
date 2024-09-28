﻿namespace Semver.Test.TestCases;

/// <summary>
/// An individual test case for whether an npm range contains a version
/// </summary>
public class NpmRangeContainsTestCase
{
    public static NpmRangeContainsTestCase NpmIncludes(string range, string version, bool includeAllPrerelease = false)
        => new NpmRangeContainsTestCase(range, includeAllPrerelease, version, true);

    public static NpmRangeContainsTestCase NpmExcludes(string range, string version, bool includeAllPrerelease = false)
        => new NpmRangeContainsTestCase(range, includeAllPrerelease, version, false);

    private NpmRangeContainsTestCase(string range, bool includeAllPrerelease, string version, bool versionIncluded)
    {
        Range = range;
        IncludeAllPrerelease = includeAllPrerelease;
        Version = SemVersion.Parse(version);
        VersionIncluded = versionIncluded;
    }

    public string Range { get; }
    public bool IncludeAllPrerelease { get; }
    public SemVersion Version { get; }
    public bool VersionIncluded { get; }

    public override string ToString()
    {
        var includes = VersionIncluded ? "includes" : "excludes";
        return $"{ToRangeString()} {includes} {Version}";
    }

    public void Assert()
    {
        var rangeSet = SemVersionRange.ParseNpm(Range, IncludeAllPrerelease);

        if (VersionIncluded)
            Xunit.Assert.True(rangeSet.Contains(Version), $"{ToRangeString()} should include {Version}");
        else
            Xunit.Assert.False(rangeSet.Contains(Version), $"{ToRangeString()} should exclude {Version}");
    }

    private string ToRangeString()
        => IncludeAllPrerelease ? Range + " w/ Prerelease" : Range;
}
