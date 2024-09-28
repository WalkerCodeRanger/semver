using System;
using System.Collections.Generic;
using Semver.Benchmarks.Builders;
using Semver.Utility;

namespace Semver.Benchmarks.Ranges;

public class NpmRangeParsingPartialHyphenRange : NpmRangeParsing
{
    private const int Seed = 1450160939;

    protected override IReadOnlyList<string> GetRanges()
    {
        var random = new Random(Seed);

        return Enumerables.Generate(RangeCount, () =>
        {
            var a = random.RandomPartialVersion(prependOperator: false);
            var b = random.RandomPartialVersion(prependOperator: false);

            return $"{a} - {b}";
        }).ToReadOnlyList();
    }
}