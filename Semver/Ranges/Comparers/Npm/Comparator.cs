using System;
using System.Text;

namespace Semver.Ranges.Comparers.Npm
{
    internal class NpmComparator
    {
        public readonly ComparatorOp Operator;
        public readonly SemVersion Version;
        public readonly bool AnyVersion;

        private readonly NpmParseOptions options;

        private string cachedStringValue;

        public NpmComparator(ComparatorOp @operator, SemVersion version, NpmParseOptions options)
        {
            if (@operator == ComparatorOp.ReasonablyClose || @operator == ComparatorOp.CompatibleWith)
                throw new ArgumentException("Invalid operator (ReasonablyClose and CompatibleWith are invalid uses in this context)");
            
            Operator = @operator;
            Version = version;
            this.options = options;
        }

        /// <summary>
        /// Any version will match when using this constructor
        /// </summary>
        public NpmComparator(NpmParseOptions options)
        {
            AnyVersion = true;
            this.options = options;
        }

        public bool Includes(SemVersion version)
        {
            if (AnyVersion)
                return true;
            
            // todo: compare versions with prerelease accounted for
            
            switch (Operator)
            {
                case ComparatorOp.LessThan:
                    return version.ComparePrecedenceTo(Version) < 0;
                case ComparatorOp.GreaterThan:
                    return version.ComparePrecedenceTo(Version) > 0;
                case ComparatorOp.LessThanOrEqualTo:
                    return version.ComparePrecedenceTo(Version) <= 0;
                case ComparatorOp.GreaterThanOrEqualTo:
                    return version.ComparePrecedenceTo(Version) >= 0;
                case ComparatorOp.Equals:
                    return version.ComparePrecedenceTo(Version) == 0;
                default:
                    throw new NotImplementedException();
            }
        }

        public override string ToString()
        {
            if (cachedStringValue != null)
                return cachedStringValue;

            var builder = new StringBuilder();
            if (AnyVersion)
            {
                builder.Append('*');
            }
            else
            {
                builder.Append(ComparatorParser.OperatorsReverse[Operator]);
                builder.Append(Version);
            }

            cachedStringValue = builder.ToString();
            return cachedStringValue;
        }
    }

    internal enum ComparatorOp
    {
        LessThan,
        GreaterThan,
        LessThanOrEqualTo,
        GreaterThanOrEqualTo,
        Equals,
        CompatibleWith,
        ReasonablyClose
    }
}
