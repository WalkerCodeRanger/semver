using System;
using System.Globalization;
using Semver.Ranges;
using Semver.Test.TestCases;

namespace Semver.Test.Builders
{
    public static class NpmRangeParsingTestCaseBuilder
    {
        public static NpmRangeParsingTestCase Valid(
            string range,
            UnbrokenSemVersionRange expectedRange,
            int maxLength = SemVersionRange.MaxRangeLength)
            => NpmRangeParsingTestCase.Valid(range, false, maxLength, SemVersionRange.Create(expectedRange));

        public static NpmRangeParsingTestCase Valid(string range, params UnbrokenSemVersionRange[] expectedRanges)
            => NpmRangeParsingTestCase.Valid(range, false, SemVersionRange.MaxRangeLength, SemVersionRange.Create(expectedRanges));

        public static NpmRangeParsingTestCase Valid(
            string range,
            bool includeAllPrerelease,
             UnbrokenSemVersionRange expectedRange,
            int maxLength = SemVersionRange.MaxRangeLength) =>
            NpmRangeParsingTestCase.Valid(range, includeAllPrerelease, maxLength, SemVersionRange.Create(expectedRange));

        public static NpmRangeParsingTestCase Valid(
            string range,
            bool includeAllPrerelease,
            params UnbrokenSemVersionRange[] expectedRanges)
            => NpmRangeParsingTestCase.Valid(range, includeAllPrerelease, SemVersionRange.MaxRangeLength, SemVersionRange.Create(expectedRanges));

        public static NpmRangeParsingTestCase Invalid<T>(
            string range,
            string exceptionMessage,
            int maxLength = SemVersionRange.MaxRangeLength) =>
            NpmRangeParsingTestCase.Invalid(range, false, maxLength, typeof(T), exceptionMessage);

        public static NpmRangeParsingTestCase Invalid(
            string range,
            string exceptionMessage = "",
            string exceptionValue = null,
            int maxLength = SemVersionRange.MaxRangeLength)
        {
            exceptionMessage = string.Format(CultureInfo.InvariantCulture, exceptionMessage, exceptionValue);
            return NpmRangeParsingTestCase.Invalid(range, false, maxLength, typeof(FormatException), exceptionMessage);
        }
    }
}
