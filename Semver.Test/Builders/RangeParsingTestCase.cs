using System;
using Semver.Ranges;

namespace Semver.Test.Builders
{
    public class RangeParsingTestCase
    {
        public static RangeParsingTestCase Valid(
            string range,
            SemVersionRangeOptions options,
            SemVersionRange expected)
            => new RangeParsingTestCase(range, options, expected);

        public static RangeParsingTestCase Invalid(
            string range,
            SemVersionRangeOptions options,
            Type exceptionType,
            string exceptionMessage)
            => new RangeParsingTestCase(range, options, exceptionType, exceptionMessage);

        private RangeParsingTestCase(string range, SemVersionRangeOptions options, SemVersionRange expected)
        {
            Range = range;
            Options = options;
            ExpectedRange = expected;
        }

        public RangeParsingTestCase(
            string range,
            SemVersionRangeOptions options,
            Type exceptionType,
            string exceptionMessage)
        {
            Range = range;
            Options = options;
            ExceptionType = exceptionType;
            ExceptionMessage = exceptionMessage;
        }

        public string Range { get; }
        public SemVersionRangeOptions Options { get; }

        #region Valid Values
        public SemVersionRange ExpectedRange { get; }
        #endregion

        #region Invalid Values
        public Type ExceptionType { get; }
        public string ExceptionMessage { get; }
        #endregion

        public override string ToString()
            => Options == SemVersionRangeOptions.Strict ? $"'{Range}'" : $"'{Range}' {Options}";
    }
}
