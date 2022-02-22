using System;
using System.Runtime.CompilerServices;

namespace Semver.Utility
{
    /// <summary>
    /// Combine hash codes in a good way since <c>System.HashCode</c> isn't available.
    /// Has a similar API to make transition easy later.
    /// </summary>
    /// <remarks>Algorithm based on HashHelpers previously used in the core CLR.
    /// https://github.com/dotnet/coreclr/blob/456afea9fbe721e57986a21eb3b4bb1c9c7e4c56/src/System.Private.CoreLib/shared/System/Numerics/Hashing/HashHelpers.cs
    /// </remarks>
    internal static class CombinedHashCode
    {
        private static readonly int RandomSeed = new Random().Next(int.MinValue, int.MaxValue);

        public static int Create<T1>(T1 value1)
            => CombineValue(RandomSeed, value1);

        public static int Create<T1, T2, T3, T4, T5>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5)
        {
            var hash = RandomSeed;
            hash = CombineValue(hash, value1);
            hash = CombineValue(hash, value2);
            hash = CombineValue(hash, value3);
            hash = CombineValue(hash, value4);
            hash = CombineValue(hash, value5);
            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int CombineValue<T>(int hash1, T value)
        {
            uint rotateLeft5 = ((uint)hash1 << 5) | ((uint)hash1 >> 27);
            return ((int)rotateLeft5 + hash1) ^ (value?.GetHashCode() ?? 0);
        }
    }
}
