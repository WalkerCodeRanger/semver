extern alias current;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Semver.Benchmarks
{
    public class Parsing : ParsingBenchmark
    {
        private const int Seed = -217274474;

        protected override IReadOnlyList<string> CreateVersions()
        {
            var random = new Random(Seed);
            return Enumerables.Generate(VersionCount,
                                      () => random.VersionString())
                                  .ToList().AsReadOnly();
        }
    }
}
