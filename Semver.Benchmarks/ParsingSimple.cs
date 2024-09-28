using System;
using System.Collections.Generic;
using Semver.Benchmarks.Builders;
using Semver.Utility;

namespace Semver.Benchmarks;

public class ParsingSimple : ParsingBenchmarks
{
    private const int Seed = 1450160939;

    protected override IReadOnlyList<string> CreateVersions()
    {
        var random = new Random(Seed);
        return Enumerables.Generate(VersionCount,
                () => random.VersionString(maxPrereleaseIdentifiers: 0, maxMetadataIdentifiers: 0))
            .ToReadOnlyList();
    }
}
