using System;
using Semver.Ranges;

namespace Semver.Test.TestCases
{
    public class RangeParsingTestCase
    {
        public static RangeParsingTestCase Valid(
            string range,
            SemVersionRangeOptions options,
            int maxLength,
            SemVersionRange expected)
            => new RangeParsingTestCase(range, options, maxLength, expected);

        public static RangeParsingTestCase Invalid(
            string range,
            SemVersionRangeOptions options,
            int maxLength,
            Type exceptionType,
            string exceptionMessageFormat)
            => new RangeParsingTestCase(range, options, maxLength, exceptionType, exceptionMessageFormat);

        private RangeParsingTestCase(
            string range,
            SemVersionRangeOptions options,
            int maxLength,
            SemVersionRange expected)
        {
            Range = range;
            Options = options;
            MaxLength = maxLength;
            IsValid = true;
            ExpectedRange = expected;
        }

        public RangeParsingTestCase(
            string range,
            SemVersionRangeOptions options,
            int maxLength,
            Type exceptionType,
            string exceptionMessageFormat)
        {
            Range = range;
            Options = options;
            MaxLength = maxLength;
            IsValid = false;
            ExceptionType = exceptionType;
            ExceptionMessageFormat = exceptionMessageFormat;
        }

        public string Range { get; }
        public SemVersionRangeOptions Options { get; }
        public int MaxLength { get; }
        public bool IsValid { get; }

        #region Valid Values
        public SemVersionRange ExpectedRange { get; }
        #endregion

        #region Invalid Values
        public Type ExceptionType { get; }
        public string ExceptionMessageFormat { get; }
        #endregion

        public override string ToString()
            => Options == SemVersionRangeOptions.Strict ? $"'{Range}'" : $"'{Range}' {Options}";
    }
}
