using System;
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

        public static NpmRangeParsingTestCase Invalid(
            string range,
            bool includeAllPrerelease,
            Type exceptionType,
            string exceptionMessage)
            => new NpmRangeParsingTestCase(range, includeAllPrerelease, exceptionType, exceptionMessage);

        private NpmRangeParsingTestCase(
            string range,
            bool includeAllPrerelease,
            SemVersionRange expected)
        {
            Range = range;
            IncludeAllPrerelease = includeAllPrerelease;
            IsValid = true;
            ExpectedRange = expected;
        }

        private NpmRangeParsingTestCase(
            string range,
            bool includeAllPrerelease,
            Type exceptionType,
            string exceptionMessageFormat)
        {
            Range = range;
            IncludeAllPrerelease = includeAllPrerelease;
            IsValid = false;
            ExceptionType = exceptionType;
            ExceptionMessageFormat = exceptionMessageFormat;
        }

        public string Range { get; }
        public bool IncludeAllPrerelease { get; }
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
