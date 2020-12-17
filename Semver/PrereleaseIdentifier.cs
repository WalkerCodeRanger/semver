using System;

namespace Semver
{
    public readonly struct PrereleaseIdentifier : IEquatable<PrereleaseIdentifier>
    {
        public string Value { get; }
        public int? IntValue { get; }

        internal PrereleaseIdentifier(string value, int? intValue)
        {
            Value = value;
            IntValue = intValue;
        }

        // TODO add public constructors

        #region Equality
        public bool Equals(PrereleaseIdentifier other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is PrereleaseIdentifier other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value?.GetHashCode() ?? 0;
        }

        public static bool operator ==(PrereleaseIdentifier left, PrereleaseIdentifier right)
        {
            return left.Value == right.Value;
        }

        public static bool operator !=(PrereleaseIdentifier left, PrereleaseIdentifier right)
        {
            return left.Value != right.Value;
        }
        #endregion

        public static implicit operator string(PrereleaseIdentifier prereleaseIdentifier)
        {
            return prereleaseIdentifier.Value;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
