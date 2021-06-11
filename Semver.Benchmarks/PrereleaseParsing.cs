using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace Semver.Benchmarks
{
    [SimpleJob(RuntimeMoniker.Net461)]
    [SimpleJob(RuntimeMoniker.NetCoreApp21)]
    public class PrereleaseParsing
    {
        private const int Seed = 37041887;
        private const int VersionCount = 1_000;
        private readonly IReadOnlyList<string> versions;

        public PrereleaseParsing()
        {
            var random = new Random(Seed);
            versions = Enumerables.Generate(VersionCount,
                                   () => random.VersionString(maxMetadataIdentifiers: 0))
                               .ToList().AsReadOnly();
        }

        [Benchmark(OperationsPerInvoke = VersionCount)]
        [Arguments(true)]
        [Arguments(false)]
        public long CurrentRegExParsing(bool strict)
        {
            // The accumulator ensures the versions aren't dead code with minimal overhead
            long accumulator = 0;
            for (int i = 0; i < VersionCount; i++)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                var version = SemVersion.Parse(this.versions[i], strict);
#pragma warning restore CS0618 // Type or member is obsolete
                accumulator += version.Major;
            }

            return accumulator;
        }

        [Benchmark(OperationsPerInvoke = VersionCount)]
        [Arguments(SemVersionStyles.Strict)]
        [Arguments(SemVersionStyles.Any)]
        public long CurrentParsing(SemVersionStyles style)
        {
            // The accumulator ensures the versions aren't dead code with minimal overhead
            long accumulator = 0;
            for (int i = 0; i < VersionCount; i++)
            {
                var version = SemVersion.Parse(versions[i], style);
                accumulator += version.Major;
            }

            return accumulator;
        }
    }
}
