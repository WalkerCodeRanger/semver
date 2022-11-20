using Semver.Ranges;

namespace Semver.Test.TestCases
{
    public class NpmRangeParsingTestCase
    {
        public static NpmRangeParsingTestCase Valid(
            string range,
            bool includeAllPrerelease,
            SemVersionRange expected)
            => new NpmRangeParsingTestCase(range, includeAllPrerelease, expected);

        private NpmRangeParsingTestCase(
            string range,
            bool includeAllPrerelease,
            SemVersionRange expected)
        {
            Range = range;
            IncludeAllPrerelease = includeAllPrerelease;
            ExpectedRange = expected;
        }

        public string Range { get; }
        public bool IncludeAllPrerelease { get; }
        public bool IsValid { get; }

        #region Valid Values
        public SemVersionRange ExpectedRange { get; }
        #endregion

        public override string ToString()
            => !IncludeAllPrerelease ? $"'{Range}'" : $"'{Range}' true";
    }
}
