using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace Semver.Benchmarks;

/// <remarks>Demonstrates why it is worth having the <see cref="SemVersion.Satisfies(UnbrokenSemVersionRange)"/>
/// overload rather than relying on implicit conversion to <see cref="Predicate{T}"/> of
/// <see cref="SemVersion"/>.</remarks>
[SimpleJob(RuntimeMoniker.Net471)]
[SimpleJob(RuntimeMoniker.NetCoreApp31)]
[SimpleJob(RuntimeMoniker.Net60)]
public class SatisfiesUnbrokenRange
{
    public static readonly SemVersion Version = SemVersion.ParsedFrom(1, 2, 3, "rc.0", "af541cd");
    public static readonly UnbrokenSemVersionRange UnbrokenRange = UnbrokenSemVersionRange.AtLeast(new SemVersion(1, 0, 0));

    [Benchmark]
    public bool SatisfiesAsPredicate()
        => Version.Satisfies((Predicate<SemVersion>)UnbrokenRange);

    [Benchmark(Baseline = true)]
    public bool Satisfies() => Version.Satisfies(UnbrokenRange);
}
