using System;
using Semver.Ranges;

namespace Semver.Test.TestCases
{
    public class NpmRangeParsingTestCase
    {
        public static NpmRangeParsingTestCase Valid(
            string range,
            bool includeAllPrerelease,
            int maxLength,
            SemVersionRange expected)
            => new NpmRangeParsingTestCase(range, includeAllPrerelease, maxLength, expected);

        public static NpmRangeParsingTestCase Invalid(
            string range,
            bool includeAllPrerelease,
            int maxLength,
            Type exceptionType,
            string exceptionMessage)
            => new NpmRangeParsingTestCase(range, includeAllPrerelease, maxLength, exceptionType, exceptionMessage);

        private NpmRangeParsingTestCase(
            string range,
            bool includeAllPrerelease,
            int maxLength,
            SemVersionRange expected)
        {
            Range = range;
            IncludeAllPrerelease = includeAllPrerelease;
            MaxLength = maxLength;
            IsValid = true;
            ExpectedRange = expected;
        }

        private NpmRangeParsingTestCase(
            string range,
            bool includeAllPrerelease,
            int maxLength,
            Type exceptionType,
            string exceptionMessageFormat)
        {
            Range = range;
            IncludeAllPrerelease = includeAllPrerelease;
            MaxLength = maxLength;
            IsValid = false;
            ExceptionType = exceptionType;
            ExceptionMessageFormat = exceptionMessageFormat;
        }

        public string Range { get; }
        public bool IncludeAllPrerelease { get; }
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
            => !IncludeAllPrerelease ? $"'{Range}'" : $"'{Range}' true";
    }
}
