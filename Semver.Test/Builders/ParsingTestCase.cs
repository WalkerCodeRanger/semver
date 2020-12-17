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

        private ParsingTestCase(string version, SemVersionStyles requiredStyles, int major, int minor, int patch, IEnumerable<PrereleaseIdentifier> prereleaseIdentifiers, IEnumerable<string> metadataIdentifiers)
        {
            Version = version;
            Style = requiredStyles;
            IsValid = true;
            Major = major;
            Minor = minor;
            Patch = patch;
            PrereleaseIdentifiers = prereleaseIdentifiers.ToList().AsReadOnly();
            MetadataIdentifiers = metadataIdentifiers.ToList().AsReadOnly();
        }

        public string Version { get; }
        public SemVersionStyles Style { get; }
        public bool IsValid { get; }

        #region Valid Values
        public int Major { get; }
        public int Minor { get; }
        public int Patch { get; }

        public IReadOnlyList<PrereleaseIdentifier> PrereleaseIdentifiers { get; }
        public IReadOnlyList<string> MetadataIdentifiers { get; }
        #endregion

        public override string ToString()
        {
            return $"{Version} as {Style}";
        }
    }
}
