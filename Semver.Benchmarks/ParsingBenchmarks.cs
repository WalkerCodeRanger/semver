﻿extern alias previous;
using System.Collections.Generic;
using System.Numerics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Previous = previous::Semver;

namespace Semver.Benchmarks;

/// <summary>
/// Base class for parsing benchmarks so they share the set of benchmarked methods and
/// other config.
/// </summary>
[SimpleJob(RuntimeMoniker.Net471)]
[SimpleJob(RuntimeMoniker.NetCoreApp31)]
[SimpleJob(RuntimeMoniker.Net60)]
public abstract class ParsingBenchmarks
{
    protected const int VersionCount = 1_000;
    private readonly IReadOnlyList<string> versions;

    protected ParsingBenchmarks()
    {
        versions = CreateVersions();
    }

    protected abstract IReadOnlyList<string> CreateVersions();

    [Params(true, false)]
    public bool Strict { get; set; }

    private SemVersionStyles currentStyles;
    private Previous.SemVersionStyles previousStyles;

    /// <summary>
    /// Construct the <see cref="SemVersionStyles"/> for for parsing.
    /// </summary>
    /// <remarks>Having <see cref="SemVersionStyles"/> as actual parameter values to benchmarks
    /// causes compile errors when running those benchmarks. Whatever Benchmark.NET is doing
    /// doesn't understand the alias for previous version.</remarks>
    [GlobalSetup]
    public void SetupStyles()
    {
        currentStyles = Strict ? SemVersionStyles.Strict : SemVersionStyles.Any;
        previousStyles = Strict ? Previous.SemVersionStyles.Strict : Previous.SemVersionStyles.Any;
    }

    [Benchmark(OperationsPerInvoke = VersionCount, Baseline = true)]
    public BigInteger Parse_Previous()
    {
        // The accumulator ensures the versions aren't dead code with minimal overhead
        BigInteger accumulator = 0;
        for (int i = 0; i < VersionCount; i++)
        {
            var version = Previous.SemVersion.Parse(versions[i], previousStyles, maxLength: int.MaxValue);
            accumulator += version.Major;
        }

        return accumulator;
    }

    [Benchmark(OperationsPerInvoke = VersionCount)]
    public BigInteger TryParse_Previous()
    {
        // The accumulator ensures the versions aren't dead code with minimal overhead
        BigInteger accumulator = 0;
        for (int i = 0; i < VersionCount; i++)
        {
            if(Previous.SemVersion.TryParse(versions[i], previousStyles, out var version, maxLength: int.MaxValue))
                accumulator += version.Major;
        }

        return accumulator;
    }

    [Benchmark(OperationsPerInvoke = VersionCount)]
    public BigInteger Parse_Current()
    {
        // The accumulator ensures the versions aren't dead code with minimal overhead
        BigInteger accumulator = 0;
        for (int i = 0; i < VersionCount; i++)
        {
            var version = SemVersion.Parse(versions[i], currentStyles, maxLength: int.MaxValue);
            accumulator += version.Major;
        }

        return accumulator;
    }

    [Benchmark(OperationsPerInvoke = VersionCount)]
    public BigInteger TryParse_Current()
    {
        // The accumulator ensures the versions aren't dead code with minimal overhead
        BigInteger accumulator = 0;
        for (int i = 0; i < VersionCount; i++)
        {
            if(SemVersion.TryParse(versions[i], currentStyles, out var version, maxLength: int.MaxValue))
                accumulator += version.Major;
        }

        return accumulator;
    }
}
