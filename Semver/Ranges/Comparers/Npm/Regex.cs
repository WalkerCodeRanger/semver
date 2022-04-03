using System.Text.RegularExpressions;

namespace Semver.Ranges.Comparers.Npm
{
    internal static class Rgx
    {
        #if COMPILED_REGEX
        private static readonly RegexOptions CompiledOptions = RegexOptions.Compiled;
        #else
        private static readonly RegexOptions CompiledOptions = RegexOptions.None;
        #endif

        public static readonly Regex Operator = new Regex(@"(?'operator'(?:<=|<|>=|>|\^|~>|~|=){0,1})", CompiledOptions);

        public static readonly Regex PartialVersion = new Regex(
            @"(?'major'x|\*|0|[1-9]\d*)" +
            @"(?:\.(?'minor'x|\*|0|[1-9]\d*)){0,1}" +
            @"(?:\.(?'patch'x|\*|0|[1-9]\d*)){0,1}" +
            @"(?:-(?'prerelease'(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))" +
            @"?(?:\+(?'buildmetadata'[0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?",
            CompiledOptions
        );

        public static readonly Regex HyphenRange = new Regex($@"^(?'minVersion'(?:{PartialVersion}))\s*-\s*(?'maxVersion'(?:{PartialVersion}))$", CompiledOptions);
        public static readonly Regex OperatorRange = new Regex($@"{Operator}(?:\s*)(?'version'{PartialVersion})\s*", CompiledOptions);
    }
}
