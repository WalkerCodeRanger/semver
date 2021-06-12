extern alias current;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Semver.Benchmarks
{
    public class PrereleaseParsing : ParsingBenchmark
    {
        private const int Seed = 37041887;

        protected override IReadOnlyList<string> CreateVersions()
        {
            var random = new Random(Seed);
            return Enumerables.Generate(VersionCount,
                                   () => random.VersionString(maxMetadataIdentifiers: 0))
                               .ToList().AsReadOnly();
        }
    }
}
