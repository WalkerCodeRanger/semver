using System;
using System.Linq;

namespace Semver.Ranges.Comparers.Npm
{
    internal class RangeParser
    {
        internal static readonly string[] OrSeparator = { "||" };

        public NpmRange ParseRange(string range, NpmParseOptions options)
        {
            string[] comps = range.Split(OrSeparator, StringSplitOptions.None);
            var comparators = comps.Select(comp => new ComparatorParser().ParseComparators(comp.Trim(), options).ToArray());

            return new NpmRange(comparators);
        }
    }
}
