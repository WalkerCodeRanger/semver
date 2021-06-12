extern alias current;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Semver.Benchmarks
{
    public class SimpleParsing : ParsingBenchmark
    {
        private const int Seed = 1450160939;

        protected override IReadOnlyList<string> CreateVersions()
        {
            var random = new Random(Seed);
            return Enumerables.Generate(VersionCount,
                                      () => random.VersionString(maxMetadataIdentifiers: 0))
                                  .ToList().AsReadOnly();
        }
    }
}
