using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Semver.Utility
{
    /// <summary>
    /// Combine hash codes in a good way since <c>System.HashCode</c> isn't available.
    /// </summary>
    /// <remarks>Algorithm based on HashHelpers previously used in the core CLR.
    /// https://github.com/dotnet/coreclr/blob/456afea9fbe721e57986a21eb3b4bb1c9c7e4c56/src/System.Private.CoreLib/shared/System/Numerics/Hashing/HashHelpers.cs
    /// </remarks>
    internal struct CombinedHashCode
    {
        private static readonly int RandomSeed = new Random().Next(int.MinValue, int.MaxValue);

        #region Create Methods
        public static CombinedHashCode Create<T1>(T1 value1)
            => new CombinedHashCode(CombineValue(RandomSeed, value1));

        public static CombinedHashCode Create<T1, T2>(T1 value1, T2 value2)
        {
            var hash = RandomSeed;
            hash = CombineValue(hash, value1);
            hash = CombineValue(hash, value2);
            return new CombinedHashCode(hash);
        }

        public static CombinedHashCode Create<T1, T2, T3>(T1 value1, T2 value2, T3 value3)
        {
            var hash = RandomSeed;
            hash = CombineValue(hash, value1);
            hash = CombineValue(hash, value2);
            hash = CombineValue(hash, value3);
            return new CombinedHashCode(hash);
        }

        public static CombinedHashCode Create<T1, T2, T3, T4>(T1 value1, T2 value2, T3 value3, T4 value4)
        {
            var hash = RandomSeed;
            hash = CombineValue(hash, value1);
            hash = CombineValue(hash, value2);
            hash = CombineValue(hash, value3);
            hash = CombineValue(hash, value4);
            return new CombinedHashCode(hash);
        }

        public static CombinedHashCode Create<T1, T2, T3, T4, T5>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5)
        {
            var hash = RandomSeed;
            hash = CombineValue(hash, value1);
            hash = CombineValue(hash, value2);
            hash = CombineValue(hash, value3);
            hash = CombineValue(hash, value4);
            hash = CombineValue(hash, value5);
            return new CombinedHashCode(hash);
        }
        #endregion

#if DEBUG
        private static readonly string UninitializedMessage = $"DEBUG: Uninitiated {nameof(CombinedHashCode)}.";
        private readonly bool initialized;
#endif
        private int hash;

        private CombinedHashCode(int hash)
        {
#if DEBUG
            initialized = true;
#endif
            this.hash = hash;
        }

        public void Add<T>(T value)
        {
#if DEBUG
            if (!initialized) throw new InvalidOperationException(UninitializedMessage);
#endif
            hash = CombineValue(hash, value);
        }

        public static implicit operator int(CombinedHashCode hashCode)
        {
#if DEBUG
            if (!hashCode.initialized) throw new InvalidOperationException(UninitializedMessage);
#endif
            return hashCode.hash;
        }

        [EditorBrowsable(EditorBrowsableState.Never), Obsolete("HashCode is a mutable struct and should not be compared with other HashCodes. Use ToHashCode to retrieve the computed hash code.", true)]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
        public override int GetHashCode() => throw new NotSupportedException();
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int CombineValue<T>(int hash1, T value)
        {
            uint rotateLeft5 = ((uint)hash1 << 5) | ((uint)hash1 >> 27);
            return ((int)rotateLeft5 + hash1) ^ (value?.GetHashCode() ?? 0);
        }
    }
}
