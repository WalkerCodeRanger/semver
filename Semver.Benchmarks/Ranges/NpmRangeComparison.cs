using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Semver.Benchmarks.Builders;
using Semver.Utility;

namespace Semver.Benchmarks.Ranges;

[SimpleJob(RuntimeMoniker.Net471)]
[SimpleJob(RuntimeMoniker.NetCoreApp31)]
[SimpleJob(RuntimeMoniker.Net60)]
public class NpmRangeComparison
{
    private const int RangeCount = 1_000;
    private const int Seed = 1450160939;
    private readonly IReadOnlyList<SemVersionRange> ranges;
    private readonly IReadOnlyList<SemVersionRange> prereleaseRanges;
    private static readonly SemVersion Version = new SemVersion(1, 0, 0);

    public NpmRangeComparison()
    {
        var random = new Random(Seed);

        SemVersionRange CreateRange(bool includePrerelease)
        {
            string strRange = random.RandomPartialVersion(prependOperator: true);
            SemVersionRange range = SemVersionRange.ParseNpm(strRange, includePrerelease);

            return range;
        }

        ranges = Enumerables.Generate(RangeCount, () => CreateRange(false)).ToReadOnlyList();
        prereleaseRanges = Enumerables.Generate(RangeCount, () => CreateRange(true)).ToReadOnlyList();
    }

    [Benchmark(OperationsPerInvoke = RangeCount, Description = "Excluding prerelease")]
    public long Parse()
    {
        // The accumulator ensures the versions aren't dead code with minimal overhead
        long accumulator = 0;
        for (int i = 0; i < ranges.Count; ++i)
        {
            var contains = ranges[i].Contains(Version);
            accumulator += contains ? 1 : 0;
        }
        return accumulator;
    }

    [Benchmark(OperationsPerInvoke = RangeCount, Description = "Including prerelease")]
    public long ParsePrerelease()
    {
        // The accumulator ensures the versions aren't dead code with minimal overhead
        long accumulator = 0;
        for (int i = 0; i < prereleaseRanges.Count; ++i)
        {
            var contains = prereleaseRanges[i].Contains(Version);
            accumulator += contains ? 1 : 0;
        }
        return accumulator;
    }
}