using System.ComponentModel;

namespace Core.Lab_1.Lab_1_1;

public static class PBlock
{
    public static byte[] Permutate(byte[] array, byte[] pBlock, IndexBitsRule rule)
    {
        var countBits = array.Length * 8;
        var outputBitLength = pBlock.Length;
        var result = new byte[array.Length];

        for (var i = 0; i < outputBitLength; ++i)
        {
            var index = pBlock[i];

            var indexInLsb0Format = _ChangeIndexToLsb0(index, countBits, rule);
            var indexInMsb0Format = _ChangeIndexFromLsb0ToMsb0(indexInLsb0Format, countBits);
            
            var byteNumber = indexInMsb0Format / 8;
            var bitNumber = indexInLsb0Format % 8;

            var correctIndex = rule is IndexBitsRule.FromLsb1 or IndexBitsRule.FromMsb1 ? i + 1 : i;
            var resultIndexInLsb0 = _ChangeIndexToLsb0(correctIndex, countBits, rule);
            var resultIndexInMsb0 = _ChangeIndexFromLsb0ToMsb0(resultIndexInLsb0, countBits);

            var resultByteNumber = resultIndexInMsb0 / 8;
            var resultBitNumber = resultIndexInLsb0 % 8;
            
            var value = _GetBit(array[byteNumber], bitNumber);
            _SetBit(ref result[resultByteNumber], value, resultBitNumber);
        }
        
        return result;
    }
    
    private static uint _GetBit(byte sourceByte, int indexInLsb0)
    {
        return (sourceByte & (1u << indexInLsb0)) >> indexInLsb0;
    }

    private static void _SetBit(ref byte sourceByte, uint value, int indexInLsb0)
    {
        if (value > 0)
        {
            sourceByte = (byte)(sourceByte | (1u << indexInLsb0));
        }
        else
        {
            sourceByte = (byte)(sourceByte & ~(1u << indexInLsb0));
        }
    }

    private static int _ChangeIndexToLsb0(int index, int countBits, IndexBitsRule rule)
    {
        switch (rule)
        {
            case IndexBitsRule.FromLsb1:
                --index;
                break;
            case IndexBitsRule.FromMsb0:
                index = countBits - index;
                --index;
                break;
            case IndexBitsRule.FromMsb1:
                index = countBits - index;
                break;
        }

        return index;
    }

    private static int _ChangeIndexFromLsb0ToMsb0(int index, int countBits)
    {
        return countBits - index - 1;
    }
    
}