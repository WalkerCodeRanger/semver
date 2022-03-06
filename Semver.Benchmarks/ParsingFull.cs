using System;
using System.Collections.Generic;
using Semver.Benchmarks.Builders;
using Semver.Utility;

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
                .ToReadOnlyList();
        }
    }
}
