using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Semver.Benchmarks.Builders;
using Semver.Ranges;
using Semver.Ranges.Comparers.Npm;
using Semver.Utility;

namespace Semver.Benchmarks.RangeBenchmarks.Npm
{
    [SimpleJob(RuntimeMoniker.Net461)]
    [SimpleJob(RuntimeMoniker.NetCoreApp21)]
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    public class NpmRangeComparison
    {
        private const int NumRanges = 1000;
        private readonly IReadOnlyList<NpmRange> ranges;
        private readonly IReadOnlyList<NpmRange> prereleaseRanges;

        public NpmRangeComparison()
        {
            var random = new Random();

            NpmRange CreateRange(bool includePrerelease)
            {
                string strRange = random.RandomPartialVersion(prependOperator: true);
                NpmRange range = NpmRange.Parse(strRange, new NpmParseOptions(includePrerelease));

                return range;
            }

            ranges = Enumerables.Generate(NumRanges, () => CreateRange(false)).ToReadOnlyList();
            prereleaseRanges = Enumerables.Generate(NumRanges, () => CreateRange(true)).ToReadOnlyList();
        }

        [Benchmark(OperationsPerInvoke = NumRanges, Description = "Excluding prerelease")]
        public void Parse()
        {
            var version = new SemVersion(1, 0, 0);
            
            for (int i = 0; i < ranges.Count; ++i)
            {
                ranges[i].Contains(version);
            }
        }

        [Benchmark(OperationsPerInvoke = NumRanges, Description = "Including prerelease")]
        public void ParsePrerelease()
        {
            var version = new SemVersion(1, 0, 0);
            
            for (int i = 0; i < prereleaseRanges.Count; ++i)
            {
                prereleaseRanges[i].Contains(version);
            }
        }
    }
}