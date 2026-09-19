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
    
    public static byte[] CycleLeftShift(byte[] block, int shiftInBits)
    {
        var number = new BigInteger(block, isUnsigned: true);
        var blockSizeBits = block.Length * 8;
        var highBits = number >> (blockSizeBits - shiftInBits);
        var lowBits = number << shiftInBits;
        var result = highBits | lowBits;
        return result.ToByteArray();
    }
}