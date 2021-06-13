using System;
using System.Collections.Generic;
using System.Linq;

namespace Semver.Test.Builders
{
    /// <summary>
    /// Represents an individual parsing test case.
    /// </summary>
    public class ParsingTestCase
    {
        public static ParsingTestCase Valid(
            string version,
            SemVersionStyles requiredStyles,
            int major, int minor, int patch,
            IEnumerable<PrereleaseIdentifier> prereleaseIdentifiers,
            IEnumerable<string> metadataIdentifiers,
            int maxLength = SemVersion.MaxVersionLength)
        {
            return new ParsingTestCase(version, requiredStyles, major, minor, patch,
                prereleaseIdentifiers, metadataIdentifiers, maxLength);
        }
        public static ParsingTestCase Invalid(
            string version,
            SemVersionStyles styles,
            Type exceptionType,
            string exceptionMessage,
            int maxLength = SemVersion.MaxVersionLength)
        {
            return new ParsingTestCase(version, styles, exceptionType, exceptionMessage, maxLength);
        }

        private ParsingTestCase(
            string version,
            SemVersionStyles requiredStyles,
            int major,
            int minor,
            int patch,
            IEnumerable<PrereleaseIdentifier> prereleaseIdentifiers,
            IEnumerable<string> metadataIdentifiers,
            int maxLength)
        {
            Version = version;
            Styles = requiredStyles;
            IsValid = true;
            Major = major;
            Minor = minor;
            Patch = patch;
            MaxLength = maxLength;
            PrereleaseIdentifiers = prereleaseIdentifiers.ToList().AsReadOnly();
            MetadataIdentifiers = metadataIdentifiers.ToList().AsReadOnly();
        }

        private ParsingTestCase(
            string version,
            SemVersionStyles styles,
            Type exceptionType,
            string exceptionMessageFormat,
            int maxLength)
        {
            Version = version;
            Styles = styles;
            IsValid = false;
            ExceptionType = exceptionType;
            ExceptionMessageFormat = exceptionMessageFormat;
            MaxLength = maxLength;
        }

        private ParsingTestCase(
            bool isValid,
            string version,
            SemVersionStyles requiredStyles,
            int major,
            int minor,
            int patch,
            IReadOnlyList<PrereleaseIdentifier> prereleaseIdentifiers,
            IReadOnlyList<string> metadataIdentifiers,
            Type exceptionType,
            string exceptionMessageFormat,
            int maxLength)
        {
            IsValid = isValid;
            Version = version;
            Styles = requiredStyles;
            Major = major;
            Minor = minor;
            Patch = patch;
            PrereleaseIdentifiers = prereleaseIdentifiers;
            MetadataIdentifiers = metadataIdentifiers;
            ExceptionType = exceptionType;
            ExceptionMessageFormat = exceptionMessageFormat;
            MaxLength = maxLength;
        }

        public ParsingTestCase Change(string version = null, SemVersionStyles? styles = null)
        {
            return new ParsingTestCase(this.IsValid, version  ?? this.Version, styles ?? this.Styles,
                this.Major, this.Minor, this.Patch,
                this.PrereleaseIdentifiers, this.MetadataIdentifiers,
                this.ExceptionType, this.ExceptionMessageFormat, this.MaxLength);
        }

        public string Version { get; }
        public SemVersionStyles Styles { get; }
        public int MaxLength { get; }
        public bool IsValid { get; }

        #region Valid Values
        public int Major { get; }
        public int Minor { get; }
        public int Patch { get; }

        public IReadOnlyList<PrereleaseIdentifier> PrereleaseIdentifiers { get; }
        public IReadOnlyList<string> MetadataIdentifiers { get; }
        #endregion

        #region Invalid Values
        public Type ExceptionType { get; }
        public string ExceptionMessageFormat { get; }
        #endregion

        public override string ToString()
        {
            if (Version is null) return $"null as {Styles}";
            return Version.Length > 200
                ? $"Long #{Version.GetHashCode()} as {Styles}"
                : $"'{Version}' as {Styles}";
        }
    }
}
