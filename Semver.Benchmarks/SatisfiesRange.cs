using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Semver.Ranges;

namespace Semver.Benchmarks
{
    /// <remarks>Demonstrates why it is worth having the <see cref="SemVersion.Satisfies(SemVersionRange)"/>
    /// overload rather than relying on implicit conversion to <see cref="Predicate{T}"/> of
    /// <see cref="SemVersion"/>.</remarks>
    [SimpleJob(RuntimeMoniker.Net461)]
    [SimpleJob(RuntimeMoniker.NetCoreApp21)]
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    public class SatisfiesRange
    {
        private static readonly SemVersion Version = SatisfiesUnbrokenRange.Version;
        private static readonly SemVersionRange Range = SemVersionRange.Create(SatisfiesUnbrokenRange.UnbrokenRange);

        [Benchmark]
        public bool SatisfiesAsPredicate()
            => Version.Satisfies((Predicate<SemVersion>)Range);

        [Benchmark(Baseline = true)]
        public bool Satisfies() => Version.Satisfies(Range);
    }
}
