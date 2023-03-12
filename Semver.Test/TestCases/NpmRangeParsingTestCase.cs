using System;
using System.Diagnostics.CodeAnalysis;
using Semver.Test.Helpers;
using Xunit;

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
            string? range,
            bool includeAllPrerelease,
            int maxLength,
            Type exceptionType,
            string exceptionMessageFormat)
            => new NpmRangeParsingTestCase(range, includeAllPrerelease, maxLength, exceptionType, exceptionMessageFormat);

        private NpmRangeParsingTestCase(
            string? range,
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
            string? range,
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

        public string? Range { get; }
        public bool IncludeAllPrerelease { get; }
        public int MaxLength { get; }

        [MemberNotNullWhen(true, nameof(Range), nameof(ExpectedRange))]
        [MemberNotNullWhen(false, nameof(ExceptionType), nameof(ExceptionMessageFormat))]
        public bool IsValid { get; }

        #region Valid Values
        public SemVersionRange? ExpectedRange { get; }
        #endregion

        #region Invalid Values
        
        public Type? ExceptionType { get; }
        public string? ExceptionMessageFormat { get; }
        #endregion

        public void AssertParseNpm()
        {
            if (IsValid)
            {
                var range = SemVersionRange.ParseNpm(Range, IncludeAllPrerelease, MaxLength);
                Assert.Equal(ExpectedRange, range);
            }
            else
            {
                var ex = Assert.Throws(ExceptionType,
                    () => SemVersionRange.ParseNpm(Range!, IncludeAllPrerelease, MaxLength));

                var expected = ExceptionMessages.InjectRange(ExceptionMessageFormat, Range);

                if (ex is ArgumentException argumentException)
                {
                    Assert.StartsWith(expected, argumentException.Message);
                    Assert.Equal("range", argumentException.ParamName);
                }
                else
                    Assert.Equal(expected, ex.Message);
            }
        }

        public void AssertTryParseNpm()
        {
            var result = SemVersionRange.TryParseNpm(Range, IncludeAllPrerelease, out var semverRange,
                MaxLength);

            Assert.Equal(IsValid, result);

            if (IsValid)
                Assert.Equal(ExpectedRange, semverRange);
            else
                Assert.Null(semverRange);
        }

        public override string ToString()
            => !IncludeAllPrerelease ? $"'{Range}'" : $"'{Range}' true";
    }
}
