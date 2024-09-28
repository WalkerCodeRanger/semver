using System;
using System.Collections.Generic;
using Semver.Benchmarks.Builders;
using Semver.Utility;

namespace Semver.Benchmarks;

public class ParsingPrerelease : ParsingBenchmarks
{
    private const int Seed = 37041887;

    protected override IReadOnlyList<string> CreateVersions()
    {
        var random = new Random(Seed);
        return Enumerables.Generate(VersionCount,
                () => random.VersionString(maxMetadataIdentifiers: 0))
            .ToReadOnlyList();
    }
}
