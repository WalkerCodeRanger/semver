using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Semver.Ranges.Comparers.Npm
{
    internal class ComparatorParser
    {
        private enum VersionRoundingType
        {
            Zero,
            ClosestCompatible,
            ReasonablyClose,
        }

        // Note: Order matters for key-values with the same value.
        // Only the first instance will be added to OperatorsReverse, which is used for converting a comparator to a string.
        // Therefore, you should add the default value for an operator first.
        internal static readonly IDictionary<string, ComparatorOp> Operators = new Dictionary<string, ComparatorOp>
        {
            { "<", ComparatorOp.LessThan },
            { ">", ComparatorOp.GreaterThan },
            { "<=", ComparatorOp.LessThanOrEqualTo },
            { ">=", ComparatorOp.GreaterThanOrEqualTo },
            { "^", ComparatorOp.CompatibleWith },
            { "~", ComparatorOp.ReasonablyClose },
            { "~>", ComparatorOp.ReasonablyClose },
            { "", ComparatorOp.Equals }, // Equals is implied when there's no operator prefix and all numbers are defined
            { "=", ComparatorOp.Equals },
        };

        // Holds the reverse of Operators. If there's multiple keys with the same value
        // only the first instance will be added to this dictionary.
        internal static readonly IDictionary<ComparatorOp, string> OperatorsReverse;

        private static readonly SemVersion ZeroVersion = new SemVersion(0, 0, 0);
        
        static ComparatorParser()
        {
            OperatorsReverse = new Dictionary<ComparatorOp, string>();

            foreach (var kv in Operators)
            {
                if (!OperatorsReverse.ContainsKey(kv.Value))
                    OperatorsReverse.Add(kv.Value, kv.Key);
            }
        }

        public IEnumerable<NpmComparator> ParseComparators(string range, NpmParseOptions options)
        {
            if (range == string.Empty) // Empty ranges imply *
            {
                yield return new NpmComparator(options);
                yield break;
            }

            Match hyphenMatch = Rgx.HyphenRange.Match(range);
            if (hyphenMatch.Success)
            {
                ParseHyphenRange(hyphenMatch, options, out var minComp, out var maxComp);

                yield return minComp;
                yield return maxComp;
                
                yield break;
            }

            MatchCollection operatorRanges = Rgx.OperatorRange.Matches(range);
            foreach (Match rangeMatch in operatorRanges)
            {
                IEnumerable<NpmComparator> comps = ParseOperatorRange(rangeMatch, options);

                foreach (var comp in comps)
                {
                    yield return comp;
                }
            }

            if (operatorRanges.Count == 0)
                throw new RangeParseException($"Unknown range syntax: {range}");
        }

        private void ParseHyphenRange(Match match, NpmParseOptions options, out NpmComparator minComparator, out NpmComparator maxComparator)
        {
            ParseVersion(match.Groups["minVersion"].Value, out var minMajor, out var minMinor, out var minPatch, out string minPrerelease, out string minMetadata);
            ParseVersion(match.Groups["maxVersion"].Value, out var maxMajor, out var maxMinor, out var maxPatch, out string maxPrerelease, out string maxMetadata);
            
            if (minMajor == null)
            {
                minComparator = new NpmComparator(options);
            }
            else
            {
                RoundVersion(VersionRoundingType.Zero, ref minMajor, ref minMinor, ref minPatch);
                SemVersion minVersion = SemVersion.ParsedFrom(minMajor.Value, minMinor.Value, minPatch.Value, minPrerelease, minMetadata);
                minComparator = new NpmComparator(ComparatorOp.GreaterThanOrEqualTo, minVersion, options);
            }

            if (maxMajor == null)
            {
                maxComparator = new NpmComparator(options);
            }
            else
            {
                ComparatorOp op = ComparatorOp.LessThanOrEqualTo;
                
                if (maxMinor == null || maxPatch == null)
                {
                    RoundVersion(VersionRoundingType.ClosestCompatible, ref maxMajor, ref maxMinor, ref maxPatch);
                    op = ComparatorOp.LessThan;
                }

                SemVersion maxVersion = SemVersion.ParsedFrom(maxMajor.Value, maxMinor.Value, maxPatch.Value, maxPrerelease, maxMetadata);
                maxComparator = new NpmComparator(op, maxVersion, options);
            }
        }

        private IEnumerable<NpmComparator> ParseOperatorRange(Match match, NpmParseOptions options)
        {
            string strOperator = match.Groups["operator"].Value;

            if (!Operators.TryGetValue(strOperator, out ComparatorOp op))
                op = ComparatorOp.Equals;

            ParseVersion(match, out var major, out var minor, out var patch, out var prerelease, out var metadata);

            if (major == null)
            {
                yield return new NpmComparator(options);
                yield break;
            }
            
            if (op == ComparatorOp.Equals && minor != null && patch != null)
            {
                var semVersion = SemVersion.ParsedFrom(major.Value, minor.Value, patch.Value, prerelease, metadata);
                yield return new NpmComparator(ComparatorOp.Equals, semVersion, options);
                yield break;
            }

            if (op == ComparatorOp.GreaterThanOrEqualTo || op == ComparatorOp.GreaterThan)
            {
                if (op == ComparatorOp.GreaterThan)
                {
                    if ((minor == null || patch == null) && RoundVersion(VersionRoundingType.ClosestCompatible, ref major, ref minor, ref patch))
                    {
                        op = ComparatorOp.GreaterThanOrEqualTo;
                    }
                }
                else
                {
                    RoundVersion(VersionRoundingType.Zero, ref major, ref minor, ref patch);
                }
                
                SemVersion version = SemVersion.ParsedFrom(major.Value, minor.Value, patch.Value, prerelease, metadata);
                yield return new NpmComparator(op, version, options);
                yield break;
            }

            if (op == ComparatorOp.LessThanOrEqualTo || op == ComparatorOp.LessThan)
            {
                if (op == ComparatorOp.LessThan)
                {
                    RoundVersion(VersionRoundingType.Zero, ref major, ref minor, ref patch);
                }
                else
                {
                    if ((minor == null || patch == null) && RoundVersion(VersionRoundingType.ClosestCompatible, ref major, ref minor, ref patch))
                    {
                        op = ComparatorOp.LessThan;
                    }
                }

                SemVersion version = SemVersion.ParsedFrom(major.Value, minor.Value, patch.Value, prerelease, metadata);
                yield return new NpmComparator(op, version, options);
                yield break;
            }

            if (op == ComparatorOp.CompatibleWith || op == ComparatorOp.ReasonablyClose)
            {
                int? minMajor = major, minMinor = minor, minPatch = patch;
                RoundVersion(VersionRoundingType.Zero, ref minMajor, ref minMinor, ref minPatch);

                int? maxMajor = major, maxMinor = minor, maxPatch = patch;
                RoundVersion(op == ComparatorOp.CompatibleWith ? VersionRoundingType.ClosestCompatible : VersionRoundingType.ReasonablyClose, ref maxMajor, ref maxMinor, ref maxPatch);

                var minVersion = SemVersion.ParsedFrom(minMajor.Value, minMinor.Value, minPatch.Value, prerelease, metadata);
                var maxVersion = SemVersion.ParsedFrom(maxMajor.Value, maxMinor.Value, maxPatch.Value);

                if (minVersion.ComparePrecedenceTo(ZeroVersion) != 0)
                    yield return new NpmComparator(ComparatorOp.GreaterThanOrEqualTo, minVersion, options);
                
                yield return new NpmComparator(ComparatorOp.LessThan, maxVersion, options);
                
                yield break;
            }

            // Partial version that expands into multiple comparators, for example "1.x" or "0.1.x" or "1"
            {
                int? minMajor = major, minMinor = minor, minPatch = patch;
                RoundVersion(VersionRoundingType.Zero, ref minMajor, ref minMinor, ref minPatch);

                int? maxMajor = major, maxMinor = minor, maxPatch = patch;
                // 0.0.x rounds to next minor version and 0.x.x rounds to next major version, regardless of whether major is 0 or not.
                if (minor == null)
                    RoundVersion(VersionRoundingType.ClosestCompatible, ref maxMajor, ref maxMinor, ref maxPatch);
                else
                    RoundVersion(VersionRoundingType.ReasonablyClose, ref maxMajor, ref maxMinor, ref maxPatch);

                var minVersion = SemVersion.ParsedFrom(minMajor.Value, minMinor.Value, minPatch.Value);
                var maxVersion = SemVersion.ParsedFrom(maxMajor.Value, maxMinor.Value, maxPatch.Value);

                if (minVersion.ComparePrecedenceTo(ZeroVersion) != 0)
                    yield return new NpmComparator(ComparatorOp.GreaterThanOrEqualTo, minVersion, options);

                yield return new NpmComparator(ComparatorOp.LessThan, maxVersion, options);
            }
        }

        private void ParseVersion(string version, out int? major, out int? minor, out int? patch, out string prerelease, out string metadata)
        {
            var match = Rgx.PartialVersion.Match(version);
            ParseVersion(match, out major, out minor, out patch, out prerelease, out metadata);
        }

        private void ParseVersion(Match match, out int? major, out int? minor, out int? patch, out string prerelease, out string metadata)
        {
            major = TryParseInt(match.Groups["major"].Value);
            minor = TryParseInt(match.Groups["minor"].Value);
            patch = TryParseInt(match.Groups["patch"].Value);
            prerelease = match.Groups["prerelease"].Value;
            metadata = match.Groups["metadata"].Value;
        }

        private bool RoundVersion(VersionRoundingType roundingType, ref int? major, ref int? minor, ref int? patch)
        {
            if (major == null)
                throw new ArgumentException("Major can not be null");

            // Special case where ~0 or ~1 etc increments major version always
            if (roundingType == VersionRoundingType.ReasonablyClose && minor == null)
            {
                major += 1;
                minor = 0;
                patch = 0;
                return true;
            }
            
            bool changed = false;

            if (minor == null)
            {
                minor = 0;
                patch = 0;
            }
            else if (patch == null)
            {
                patch = 0;
            }
            
            switch (roundingType)
            {
                case VersionRoundingType.Zero: return false;
                case VersionRoundingType.ClosestCompatible:
                {
                    // Missing values are raised to the next major/minor
                    // Some examples:
                    // 1.0.0 --> 2.0.0
                    // 0.1.0 --> 0.2.0 (0 major is only compatible with versions of the same minor number)
                    // 0.0.1 --> 0.0.2 (it's only compatible version is itself)
                    if (major == 0 && minor == 0) // 0.0.1 --> 0.0.2
                    {
                        patch += 1;
                        changed = true;
                    }
                    else if (major == 0 && minor > 0) // 0.1.x --> 0.2.0
                    {
                        minor += 1;
                        patch = 0;
                        changed = true;
                    }
                    else if (major > 0) // (n>0).x.x --> n+1.0.0
                    {
                        major += 1;
                        minor = 0;
                        patch = 0;
                        changed = true;
                    }

                    break;
                }
                case VersionRoundingType.ReasonablyClose:
                {
                    // Only increment minor, without exceptions for 0.x or 0.0.x
                    // Note that this method behaves differently than the behaviour for ~, in that the version number is fully qualified (missing values are zeroed)
                    // The behaviour for ~1 (--> 2.0.0) is different than ~1.0.0 (--> 1.2.0)
                    // For clarification, this method does not handle ~1 ranges
                    // Some examples:
                    // 1.0.0 -> 1.1.0
                    // 0.1.0 --> 0.2.0
                    // 0.0.1 -> 0.1.0
                    minor += 1;
                    patch = 0;
                    changed = true;
                    
                    break;
                }
            }

            return changed;
        }

        private static int? TryParseInt(string strInteger)
        {
            if (!int.TryParse(strInteger, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
                return null;

            return result;
        }
    }
}
