extern alias previous;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Previous = previous::Semver;

namespace Semver.Benchmarks
{
    /// <summary>
    /// Base class for parsing benchmarks so they share the set of benchmarked methods and
    /// other config.
    /// </summary>
    [SimpleJob(RuntimeMoniker.Net461)]
    [SimpleJob(RuntimeMoniker.NetCoreApp21)]
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    public abstract class ParsingBenchmarks
    {
        protected const int VersionCount = 1_000;
        private readonly IReadOnlyList<string> versions;

        protected ParsingBenchmarks()
        {
            versions = CreateVersions();
        }

        protected abstract IReadOnlyList<string> CreateVersions();

        [Benchmark(OperationsPerInvoke = VersionCount)]
        [Arguments(true)]
        [Arguments(false)]
        public long RegExParse_Previous(bool strict)
        {
            // The accumulator ensures the versions aren't dead code with minimal overhead
            long accumulator = 0;
            for (int i = 0; i < VersionCount; i++)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                var version = Previous.SemVersion.Parse(versions[i], strict);
#pragma warning restore CS0618 // Type or member is obsolete
                accumulator += version.Major;
            }

            return accumulator;
        }

        [Benchmark(OperationsPerInvoke = VersionCount)]
        [Arguments(true)]
        [Arguments(false)]
        public long RegExTryParse_Previous(bool strict)
        {
            // The accumulator ensures the versions aren't dead code with minimal overhead
            long accumulator = 0;
            for (int i = 0; i < VersionCount; i++)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                Previous.SemVersion.TryParse(versions[i], out var version, strict);
#pragma warning restore CS0618 // Type or member is obsolete
                accumulator += version.Major;
            }

            return accumulator;
        }

        [Benchmark(OperationsPerInvoke = VersionCount)]
        [Arguments(true)]
        [Arguments(false)]
        public long RegExParse_Current(bool strict)
        {
            // The accumulator ensures the versions aren't dead code with minimal overhead
            long accumulator = 0;
            for (int i = 0; i < VersionCount; i++)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                var version = SemVersion.Parse(versions[i], strict);
#pragma warning restore CS0618 // Type or member is obsolete
                accumulator += version.Major;
            }

            return accumulator;
        }

        [Benchmark(OperationsPerInvoke = VersionCount)]
        [Arguments(true)]
        [Arguments(false)]
        public long RegExTryParse_Current(bool strict)
        {
            // The accumulator ensures the versions aren't dead code with minimal overhead
            long accumulator = 0;
            for (int i = 0; i < VersionCount; i++)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                SemVersion.TryParse(versions[i], out var version, strict);
#pragma warning restore CS0618 // Type or member is obsolete
                accumulator += version.Major;
            }

            return accumulator;
        }

        [Benchmark(OperationsPerInvoke = VersionCount)]
        [Arguments(SemVersionStyles.Strict)]
        [Arguments(SemVersionStyles.Any)]
        public long Parse_Current(SemVersionStyles style)
        {
            // The accumulator ensures the versions aren't dead code with minimal overhead
            long accumulator = 0;
            for (int i = 0; i < VersionCount; i++)
            {
                var version = SemVersion.Parse(versions[i], style, maxLength: int.MaxValue);
                accumulator += version.Major;
            }

            return accumulator;
        }

        [Benchmark(OperationsPerInvoke = VersionCount)]
        [Arguments(SemVersionStyles.Strict)]
        [Arguments(SemVersionStyles.Any)]
        public long TryParse_Current(SemVersionStyles style)
        {
            // The accumulator ensures the versions aren't dead code with minimal overhead
            long accumulator = 0;
            for (int i = 0; i < VersionCount; i++)
            {
                SemVersion.TryParse(versions[i], style, out var version, maxLength: int.MaxValue);
                accumulator += version.Major;
            }

            return accumulator;
        }
    }
}
