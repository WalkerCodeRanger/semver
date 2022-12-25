using System;
using Semver.Ranges;
using Semver.Test.Helpers;
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
            string message,
            int maxLength = SemVersionRange.MaxRangeLength) =>
            NpmRangeParsingTestCase.Invalid(range, false, maxLength, typeof(T), message);

        public static NpmRangeParsingTestCase Invalid(
            string range,
            string message = "",
            string value = null,
            int maxLength = SemVersionRange.MaxRangeLength)
        {
            message = ExceptionMessages.InjectValue(message, value);
            message = ExceptionMessages.InjectVersion(message, value);
            return NpmRangeParsingTestCase.Invalid(range, false, maxLength, typeof(FormatException), message);
        }
    }
}
