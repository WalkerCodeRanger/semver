using System;
using System.Collections.Generic;
using Semver.Benchmarks.Builders;
using Semver.Utility;

namespace Semver.Benchmarks.RangeBenchmarks.Npm
{
    public class NpmRangeParsingPartialHyphenRange : NpmRangeParsing
    {
        protected override IReadOnlyList<string> GetRanges()
        {
            var random = new Random();

            return Enumerables.Generate(NumRanges, () =>
            {
                var a = random.RandomPartialVersion(prependOperator: false);
                var b = random.RandomPartialVersion(prependOperator: false);

                return $"{a} - {b}";
            }).ToReadOnlyList();
        }
    }
}