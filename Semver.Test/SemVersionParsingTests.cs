using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Semver.Test.Builders;
using Xunit;

namespace Semver.Test
{
    public class SemVersionParsingTests
    {
        //private const string InvalidSemVersionStylesMessage = "An invalid SemVersionStyles value was used.";
        //private const string LeadingWhitespaceMessage = "Version '{0}' has leading whitespace";
        //private const string EmptyVersionMessage = "Empty string";
        //private const string AllWhitespaceVersionMessage = "All whitespace";
        //private const string LeadingLowerVMessage = "Leading Lower V {0}";
        //private const string LeadingUpperVMessage = "Leading Upper V {0}";
        private const string LeadingZeroInMajorMinorOrPatchMessage = "Leading Zero in major, minor, or patch version in '{0}'";
        private const string MissingMajorMinorOrPatchMessage = "Missing major, minor, or patch version in '{0}'";
        //private const string NumberParseInvalid = "Number parse is invalid {0}";
        private const string MissingMinorMessage = "Missing minor version in '{0}'";
        private const string MissingPatchMessage = "Missing patch version in '{0}'";
        private const string MajorMinorOrPatchOverflowMessageFormat = "Major, minor, or patch version '{1}' was too large for Int32 in '{0}'";
        private const string MissingPrereleaseIdentifierMessage = "Missing prerelease identifier in '{0}'";
        private const string LeadingZeroInPrereleaseMessage = "Leading Zero in prerelease identifier in version '{0}'";
        private const string InvalidCharacterInPrereleaseMessage = "Invalid character '{1}' in prerelease identifier in '{0}'";

        public static readonly TheoryData<ParsingTestCase> ParsingTestCases = ExpandTestCases(
            // version numbers given with the link in the spec to a regex for semver versions
            Valid("0.0.4", 0, 0, 4),
            Valid("1.2.3", 1, 2, 3),
            Valid("10.20.30", 10, 20, 30),
            Valid("1.1.2-prerelease+meta", 1, 1, 2, Pre("prerelease"), "meta"),
            Valid("1.1.2+meta", 1, 1, 2, Pre(), "meta"),
            Valid("1.1.2+meta-valid", 1, 1, 2, Pre(), "meta-valid"),
            Valid("1.0.0-alpha", 1, 0, 0, Pre("alpha")),
            Valid("1.0.0-beta", 1, 0, 0, Pre("beta")),
            Valid("1.0.0-alpha.beta", 1, 0, 0, Pre("alpha", "beta")),
            Valid("1.0.0-alpha.beta.1", 1, 0, 0, Pre("alpha", "beta", "1")),
            Valid("1.0.0-alpha.1", 1, 0, 0, Pre("alpha", 1)),
            Valid("1.0.0-alpha0.valid", 1, 0, 0, Pre("alpha0", "valid")),
            Valid("1.0.0-alpha.0valid", 1, 0, 0, Pre("alpha", "0valid")),
            Valid("1.0.0-alpha-a.b-c-somethinglong+build.1-aef.1-its-okay", 1, 0, 0, Pre("alpha-a", "b-c-somethinglong"), "build.1-aef.1-its-okay"),
            Valid("1.0.0-rc.1+build.1", 1, 0, 0, Pre("rc", 1), "build.1"),
            Valid("2.0.0-rc.1+build.123", 2, 0, 0, Pre("rc", 1), "build.123"),
            Valid("1.2.3-beta", 1, 2, 3, Pre("beta")),
            Valid("10.2.3-DEV-SNAPSHOT", 10, 2, 3, Pre("DEV-SNAPSHOT")),
            Valid("1.2.3-SNAPSHOT-123", 1, 2, 3, Pre("SNAPSHOT-123")),
            Valid("1.0.0", 1),
            Valid("2.0.0", 2),
            Valid("1.1.7", 1, 1, 7),
            Valid("2.0.0+build.1848", 2, 0, 0, Pre(), "build.1848"),
            Valid("2.0.1-alpha.1227", 2, 0, 1, Pre("alpha", 1227)),
            Valid("1.0.0-alpha+beta", 1, 0, 0, Pre("alpha"), "beta"),
            Valid("1.2.3----RC-SNAPSHOT.12.9.1--.12+788", 1, 2, 3, Pre("---RC-SNAPSHOT", 12, 9, "1--", 12), "788"),
            Valid("1.2.3----R-S.12.9.1--.12+meta", 1, 2, 3, Pre("---R-S", 12, 9, "1--", 12), "meta"),
            Valid("1.2.3----RC-SNAPSHOT.12.9.1--.12", 1, 2, 3, Pre("---RC-SNAPSHOT", 12, 9, "1--", 12)),
            Valid("1.0.0+0.build.1-rc.10000aaa-kk-0.1", 1, 0, 0, Pre(), "0.build.1-rc.10000aaa-kk-0.1"),
            Valid("1.0.0-0A.is.legal", 1, 0, 0, Pre("0A", "is", "legal")),
            // This was given as a valid example, but isn't supported by the semver package because of overflow
            Invalid<OverflowException>("99999999999999999999999.999999999999999999.99999999999999999",
                MajorMinorOrPatchOverflowMessageFormat, "99999999999999999999999"),
            // These are invalid version numbers given with the link in the spec to a regex for semver versions
            Invalid("1", MissingMinorMessage),
            Invalid("1.2", MissingPatchMessage),
            Invalid("1.2.3-0123", LeadingZeroInPrereleaseMessage, "0123"),
            Invalid("1.2.3-0123.0123", LeadingZeroInPrereleaseMessage, "0123"),
            Invalid("1.1.2+.123"),
            Invalid("+invalid", MissingMajorMinorOrPatchMessage),
            Invalid("-invalid", MissingMajorMinorOrPatchMessage),
            Invalid("-invalid+invalid", MissingMajorMinorOrPatchMessage),
            Invalid("-invalid.01", MissingMajorMinorOrPatchMessage),
            Invalid("alpha", MissingMajorMinorOrPatchMessage),
            Invalid("alpha.beta", MissingMajorMinorOrPatchMessage),
            Invalid("alpha.beta.1", MissingMajorMinorOrPatchMessage),
            Invalid("alpha.1", MissingMajorMinorOrPatchMessage),
            Invalid("alpha+beta", MissingMajorMinorOrPatchMessage),
            Invalid("alpha_beta", MissingMajorMinorOrPatchMessage),
            Invalid("alpha.", MissingMajorMinorOrPatchMessage),
            Invalid("alpha..", MissingMajorMinorOrPatchMessage),
            Invalid("beta", MissingMajorMinorOrPatchMessage),
            Invalid("1.0.0-alpha_beta", InvalidCharacterInPrereleaseMessage, "_"),
            Invalid("-alpha.", MissingMajorMinorOrPatchMessage),
            Invalid("1.0.0-alpha..", MissingPrereleaseIdentifierMessage),
            Invalid("1.0.0-alpha..1", MissingPrereleaseIdentifierMessage),
            Invalid("1.0.0-alpha...1", MissingPrereleaseIdentifierMessage),
            Invalid("1.0.0-alpha....1", MissingPrereleaseIdentifierMessage),
            Invalid("1.0.0-alpha.....1", MissingPrereleaseIdentifierMessage),
            Invalid("1.0.0-alpha......1", MissingPrereleaseIdentifierMessage),
            Invalid("1.0.0-alpha.......1", MissingPrereleaseIdentifierMessage),
            Invalid("01.1.1", LeadingZeroInMajorMinorOrPatchMessage),
            Invalid("1.01.1", LeadingZeroInMajorMinorOrPatchMessage),
            Invalid("1.1.01", LeadingZeroInMajorMinorOrPatchMessage),
            Invalid("1.2", MissingPatchMessage),
            Invalid("1.2.3.DEV"),
            Invalid("1.2-SNAPSHOT", MissingPatchMessage),
            Invalid("1.2.31.2.3----RC-SNAPSHOT.12.09.1--..12+788", MissingPrereleaseIdentifierMessage),
            Invalid("1.2-RC-SNAPSHOT", MissingPatchMessage),
            Invalid("-1.0.3-gamma+b7718", MissingMajorMinorOrPatchMessage),
            Invalid("+justmeta", MissingMajorMinorOrPatchMessage),
            Invalid("9.8.7+meta+meta"),
            Invalid("9.8.7-whatever+meta+meta"),
            Invalid<OverflowException>("99999999999999999999999.999999999999999999.99999999999999999----RC-SNAPSHOT.12.09.1--------------------------------..12",
                MajorMinorOrPatchOverflowMessageFormat, "99999999999999999999999"),


            Invalid<ArgumentNullException>(null, "Value cannot be null.\r\nParameter name: version"));



        [Fact]
        public void CanConstructParsingTestCases()
        {
            _ = ParsingTestCases;
        }

        [Theory]
        [MemberData(nameof(ParsingTestCases))]
        public void ParseWithStyleParsesCorrectly(ParsingTestCase testCase)
        {
            _ = testCase ?? throw new ArgumentNullException(nameof(testCase));

            if (testCase.IsValid)
            {
                var version = SemVersion.Parse(testCase.Version, testCase.Styles);

                AssertVersionEqual(version, testCase);
            }
            else
            {
                var ex = Assert.Throws(testCase.ExceptionType,
                    () => SemVersion.Parse(testCase.Version, testCase.Styles));

                Assert.Equal(testCase.ExceptionMessage, ex.Message);
            }
        }

        [Theory]
        [MemberData(nameof(ParsingTestCases))]
        public void TryParseWithStyleParsesCorrectly(ParsingTestCase testCase)
        {
            _ = testCase ?? throw new ArgumentNullException(nameof(testCase));

            var result = SemVersion.TryParse(testCase.Version, testCase.Styles, out var version);

            if (testCase.IsValid)
            {
                Assert.True(result);
                AssertVersionEqual(version, testCase);
            }
            else
            {
                Assert.False(result);
                Assert.Null(version);
            }
        }

        private static void AssertVersionEqual(SemVersion version, ParsingTestCase testCase)
        {
            Assert.Equal(testCase.Major, version.Major);
            Assert.Equal(testCase.Minor, version.Minor);
            Assert.Equal(testCase.Patch, version.Patch);
            Assert.Equal(testCase.PrereleaseIdentifiers, version.PrereleaseIdentifiers);
            Assert.Equal(string.Join(".", testCase.PrereleaseIdentifiers), version.Prerelease);
            Assert.Equal(testCase.PrereleaseIdentifiers.Any(), version.IsPrerelease);
            Assert.Equal(testCase.MetadataIdentifiers, version.MetadataIdentifiers);
            Assert.Equal(string.Join(".", testCase.MetadataIdentifiers), version.Metadata);
#pragma warning disable 618 // Obsolete Warning
            Assert.Equal(string.Join(".", testCase.MetadataIdentifiers), version.Build);
#pragma warning restore 618
        }

        private static TheoryData<ParsingTestCase> ExpandTestCases(params ParsingTestCase[] testCases)
        {
            var theoryData = new TheoryData<ParsingTestCase>();

            foreach (var testCase in testCases)
            {
                theoryData.Add(testCase);
                // TODO construct cases for other styles
            }

            return theoryData;
        }

        private static ParsingTestCase Valid(
            string version,
            SemVersionStyles requiredStyles,
            int major,
            int minor = 0,
            int patch = 0,
            IEnumerable<PrereleaseIdentifier> prerelease = null,
            string metadata = "")
        {
            return ParsingTestCase.Valid(version, requiredStyles, major, minor, patch,
                prerelease ?? Enumerable.Empty<PrereleaseIdentifier>(),
                metadata.SplitExceptEmpty('.'));
        }

        private static ParsingTestCase Valid(
            string version,
            int major,
            int minor = 0,
            int patch = 0,
            IEnumerable<PrereleaseIdentifier> prerelease = null,
            string metadata = "")
        {
            return ParsingTestCase.Valid(version, SemVersionStyles.SemVer2, major, minor, patch,
            prerelease ?? Enumerable.Empty<PrereleaseIdentifier>(),
            metadata.SplitExceptEmpty('.'));
        }

        private static ParsingTestCase Invalid<T>(
            string version,
            SemVersionStyles requiredStyles,
            string exceptionMessage = "",
            string exceptionValue = null)
            where T : Exception
        {
            exceptionMessage = InjectValue(exceptionMessage, exceptionValue);
            return ParsingTestCase.Invalid(version, requiredStyles, typeof(T), exceptionMessage);
        }

        private static ParsingTestCase Invalid<T>(
            string version,
            string exceptionMessage = "",
            string exceptionValue = null)
            where T : Exception
        {
            exceptionMessage = InjectValue(exceptionMessage, exceptionValue);
            return ParsingTestCase.Invalid(version, SemVersionStyles.SemVer2, typeof(T), exceptionMessage);
        }

        private static ParsingTestCase Invalid(string version, string exceptionMessage = "",
            string exceptionValue = null)
        {
            exceptionMessage = InjectValue(exceptionMessage, exceptionValue);
            return ParsingTestCase.Invalid(version, SemVersionStyles.SemVer2, typeof(FormatException), exceptionMessage);
        }

        public static IEnumerable<PrereleaseIdentifier> Pre(params TestPrereleaseIdentifier[] identifiers)
        {
            return identifiers.Select(i => (PrereleaseIdentifier)i);
        }

        private static string InjectValue(string format, string value)
        {
            return string.Format(CultureInfo.InvariantCulture, format, "{0}", value);
        }
    }
}
