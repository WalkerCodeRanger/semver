using System;
using System.Collections.Generic;
using System.Linq;

namespace Semver.Test
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
            IEnumerable<string> metadataIdentifiers)
        {
            return new ParsingTestCase(version, requiredStyles, major, minor, patch, prereleaseIdentifiers, metadataIdentifiers);
        }
        public static ParsingTestCase Invalid(
            string version,
            SemVersionStyles styles,
            Type exceptionType,
            string exceptionMessage)
        {
            return new ParsingTestCase(version, styles, exceptionType, exceptionMessage);
        }

        private ParsingTestCase(string version, SemVersionStyles requiredStyles, int major, int minor, int patch, IEnumerable<PrereleaseIdentifier> prereleaseIdentifiers, IEnumerable<string> metadataIdentifiers)
        {
            Version = version;
            Styles = requiredStyles;
            IsValid = true;
            Major = major;
            Minor = minor;
            Patch = patch;
            PrereleaseIdentifiers = prereleaseIdentifiers.ToList().AsReadOnly();
            MetadataIdentifiers = metadataIdentifiers.ToList().AsReadOnly();
        }

        private ParsingTestCase(string version, SemVersionStyles styles, Type exceptionType, string exceptionMessageFormat)
        {
            Version = version;
            Styles = styles;
            IsValid = false;
            ExceptionType = exceptionType;
            ExceptionMessageFormat = exceptionMessageFormat;
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
            string exceptionMessageFormat)
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
        }

        public ParsingTestCase Change(string version = null, SemVersionStyles? styles = null)
        {
            return new ParsingTestCase(this.IsValid, version  ?? this.Version, styles ?? this.Styles,
                this.Major, this.Minor, this.Patch,
                this.PrereleaseIdentifiers, this.MetadataIdentifiers,
                this.ExceptionType, this.ExceptionMessageFormat);
        }

        public string Version { get; }
        public SemVersionStyles Styles { get; }
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
