using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Semver.Ranges.Npm;

namespace Semver.Benchmarks.RangeBenchmarks.Npm
{
    [SimpleJob(RuntimeMoniker.Net461)]
    [SimpleJob(RuntimeMoniker.NetCoreApp21)]
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    public abstract class NpmRangeParsing
    {
        protected const int NumRanges = 1000;
        private readonly IReadOnlyList<string> ranges;

        protected abstract IReadOnlyList<string> GetRanges();

        protected NpmRangeParsing()
        {
            ranges = GetRanges();
        }

        [Benchmark(OperationsPerInvoke = NumRanges)]
        [Arguments(false)]
        [Arguments(true)]
        public void Parse(bool prerelease)
        {
            foreach (var range in ranges)
                NpmRange.Parse(range, prerelease);
        }
    }
}