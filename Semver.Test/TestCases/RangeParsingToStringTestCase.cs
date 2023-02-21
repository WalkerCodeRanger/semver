namespace Semver.Test.TestCases
{
    /// <summary>
    /// Test case for parsing a range and then converting it back to a string.
    /// </summary>
    public class RangeParsingToStringTestCase
    {
        public static RangeParsingToStringTestCase Valid(
            string range,
            SemVersionRangeOptions options,
            int maxLength,
            string expected)
            => new RangeParsingToStringTestCase(range, options, maxLength, expected);

        private RangeParsingToStringTestCase(
            string range,
            SemVersionRangeOptions options,
            int maxLength,
            string expected)
        {
            Range = range;
            Options = options;
            MaxLength = maxLength;
            IsValid = true;
            ExpectedRange = expected;
        }

        public string Range { get; }
        public SemVersionRangeOptions Options { get; }
        public int MaxLength { get; }
        public bool IsValid { get; }

        #region Valid Values
        public string ExpectedRange { get; }
        #endregion

        public override string ToString() =>
            Options == SemVersionRangeOptions.Strict ? $"'{Range}'" : $"'{Range}' {Options}";
    }
}
