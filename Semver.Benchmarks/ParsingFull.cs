using System;
using System.Collections.Generic;
using System.Linq;
using Semver.Benchmarks.Builders;

namespace Semver.Benchmarks
{
    public class ParsingFull : Parsing
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
