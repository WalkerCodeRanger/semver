using System;
using System.Globalization;
using Semver.Ranges;
using Semver.Test.TestCases;

namespace Semver.Test.Builders
{
    public static class NpmRangeParsingTestCaseBuilder
    {
        public static NpmRangeParsingTestCase Valid(string range, params UnbrokenSemVersionRange[] expectedRanges)
            => NpmRangeParsingTestCase.Valid(range, false, SemVersionRange.Create(expectedRanges));

        public static NpmRangeParsingTestCase Valid(
            string range,
            bool includeAllPrerelease,
            params UnbrokenSemVersionRange[] expectedRanges)
            => NpmRangeParsingTestCase.Valid(range, includeAllPrerelease, SemVersionRange.Create(expectedRanges));

        public static NpmRangeParsingTestCase Invalid<T>(
            string range,
            string exceptionMessage) =>
            NpmRangeParsingTestCase.Invalid(range, false, typeof(T), exceptionMessage);

        public static NpmRangeParsingTestCase Invalid(
            string range,
            string exceptionMessage = "",
            string exceptionValue = null)
        {
            exceptionMessage = string.Format(CultureInfo.InvariantCulture, exceptionMessage, exceptionValue);
            return NpmRangeParsingTestCase.Invalid(range, false, typeof(FormatException), exceptionMessage);
        }
    }
}
