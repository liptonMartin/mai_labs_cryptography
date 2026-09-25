using System.Numerics;

namespace Core.Lab_1;

public static class Helper
{
    public static byte[] XorArrayOfBytes(params byte[][] arrays)
    {
        var length = arrays[0].Length;
        var result = new byte[length];
        for (var i = 0; i < length; ++i)
        {
            foreach (var array in arrays)
            {
                result[i] ^= array[i];
            }
        }

        return result;
    }

    public static uint CycleLeftShiftKBits(uint block, int shiftInBits, int k)
    {
        // shift only k bits, first (n - k) bits remain on its places
        if (k is < 1 or > 32)
            throw new ArgumentOutOfRangeException(nameof(k));
        if (shiftInBits < 0)
            throw new ArgumentOutOfRangeException(nameof(shiftInBits));

        shiftInBits %= k;

        var mask = (1u << k) - 1;
        var highBits = block & ~mask;
        block &= mask;
        return highBits | (((block << shiftInBits) | (block >> (k - shiftInBits))) & mask);
    }

    public static ulong TransformArrayBytesBigEndianToUlong(byte[] array)
    {
        if (array.Length > 8)
            throw new InvalidOperationException("array have more bytes than ulong");

        ulong block = 0;
        foreach (var b in array)
            block = (block << 8) | b;
        return block;
    }

    public static byte GetFirstKBits(ulong number, int k)
    {
        return (byte)(number >> (sizeof(ulong) * 8 - k));
    }
}