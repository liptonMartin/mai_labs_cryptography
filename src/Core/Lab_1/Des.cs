namespace Core.Lab_1;

public class DesRoundKeysGenerator : IRoundKeysGenerator
{
    public List<byte[]> GenerateRoundKeys(byte[] key)
    {
        var cdBlockBytes = _PermutateByPc_1(key);
        ulong cdBlock = 0;
        foreach (var b in cdBlockBytes)
            cdBlock = (cdBlock << 8) | b;
        var cBlock = (uint)((cdBlock >> 28) & 0x0FFF_FFFF);
        var dBlock = (uint)(cdBlock & 0x0FFF_FFFF);

        List<byte[]> roundKeys = [];

        for (var i = 1; i <= 16; ++i)
        {
            int countBitShift = 2;
            if (i is 1 or 2 or 4 or 16) countBitShift = 1;

            cBlock = Helper.CycleLeftShiftKBits(cBlock, countBitShift, 28);
            dBlock = Helper.CycleLeftShiftKBits(dBlock, countBitShift, 28);

            var ulongCBlock = (ulong)cBlock << 28;
            var ulongDBlock = (ulong)dBlock;
            cdBlock = ulongCBlock | ulongDBlock;

            var roundKey = _PermutateByPc_2(cdBlock);

            roundKeys.Add(roundKey);
        }

        return roundKeys;
    }

    private byte[] _PermutateByPc_1(byte[] block)
    {
        byte[] pBlock =
        [
            57, 49, 41, 33, 25, 17, 9,
            1, 58, 50, 42, 34, 26, 18,
            10, 2, 59, 51, 43, 35, 27,
            19, 11, 3, 60, 52, 44, 36,
            63, 55, 47, 39, 31, 23, 15,
            7, 62, 54, 46, 38, 30, 22,
            14, 6, 61, 53, 45, 37, 29,
            21, 13, 5, 28, 20, 12, 4,
        ];

        return PBlock.Permutate(block, pBlock, IndexBitsRule.FromMsb1);
    }


    private byte[] _PermutateByPc_2(ulong block)
    {
        var blockBytes = new byte[8];
        block <<= 8; // because we have only 7 bytes in ulong (8 bytes)
        for (var i = sizeof(ulong) - 1; i >= 1; --i)
        {
            var countBits = i * 8;
            var iByte = block & (255UL << countBits);
            var index = 7 - i;
            blockBytes[index] = (byte)(iByte >> countBits);
        }

        byte[] pBlock =
        [
            14, 17, 11, 24, 1, 5, 3, 28, 15, 6, 21, 10, 23, 19, 12, 4, 26, 8, 16, 7, 27, 20, 13, 2, 41, 52, 31, 37, 47,
            55, 30, 40, 51, 45, 33, 48, 44, 49, 39, 56, 34, 53, 46, 42, 50, 36, 29, 32
        ];

        return PBlock.Permutate(blockBytes, pBlock, IndexBitsRule.FromMsb1);
    }
}

public class FiestelFunction : IEncryptionRound
{
    public byte[] DoEncrypt(byte[] halfBlock, byte[] roundKey)
    {
        var expandHalfBlock = _ExpandPermutation(halfBlock);
        var xor = Helper.XorArrayOfBytes(expandHalfBlock, roundKey);

        int bitBuffer = 0;
        var bitCount = 0;

        var result = new byte[halfBlock.Length];
        var indexResult = 0;
        var indexOperation = 0;
        foreach (var b in xor)
        {
            bitBuffer = (bitBuffer << 8) | b;
            bitCount += 8;

            while (bitCount >= 6)
            {
                bitCount -= 6;
                var block6Bits = (byte)((bitBuffer >> 6) & 0b_0011_1111);
                var block4Bits = _S1Transformation(block6Bits);

                result[indexResult] |= block4Bits;
                if (indexOperation % 2 == 0)
                    result[indexResult] <<= 4;
                else
                    ++indexResult;

                ++indexOperation;
            }
        }

        return _PPermutation(result);
    }

    private byte _S1Transformation(byte block6Bits)
    {
        byte[,] sBlock =
        {
            { 14, 4, 13, 1, 2, 15, 11, 8, 3, 10, 6, 12, 5, 9, 0, 7 },
            { 0, 15, 7, 4, 14, 2, 13, 1, 10, 6, 12, 11, 9, 5, 3, 8 },
            { 4, 1, 14, 8, 13, 6, 2, 11, 15, 12, 9, 7, 3, 10, 5, 0 },
            { 15, 12, 8, 2, 4, 9, 1, 7, 5, 11, 3, 14, 10, 0, 6, 13 },
        };

        var column = (block6Bits & 0b_0001_1110) >> 1;
        var row = ((block6Bits & 0b_0010_0000) >> 4) | (block6Bits & 1);

        return sBlock[row, column];
    }

    private byte[] _ExpandPermutation(byte[] block)
    {
        byte[] pBlock =
        [
            32, 1, 2, 3, 4, 5,
            4, 5, 6, 7, 8, 9,
            8, 9, 10, 11, 12, 13,
            12, 13, 14, 15, 16, 17,
            16, 17, 18, 19, 20, 21,
            20, 21, 22, 23, 24, 25,
            24, 25, 26, 27, 28, 29,
            28, 29, 30, 31, 32, 1
        ];

        return PBlock.Permutate(block, pBlock, IndexBitsRule.FromMsb1);
    }

    private byte[] _PPermutation(byte[] block)
    {
        byte[] pBlock =
        [
            16, 7, 20, 21,
            29, 12, 28, 17,
            1, 15, 23, 26,
            5, 18, 31, 10,
            2, 8, 24, 14,
            32, 27, 3, 9,
            19, 13, 30, 6,
            22, 11, 4, 25,
        ];

        return PBlock.Permutate(block, pBlock, IndexBitsRule.FromMsb1);
    }
}

public class Des : ISymmetricalEncryptDecrypt
{
    private readonly FiestelCipher _fiestelCipher;

    public Des(byte[] key)
    {
        _fiestelCipher = new FiestelCipher(new DesRoundKeysGenerator(), new FiestelFunction());
        _fiestelCipher.GenerateRoundKeys(key);
    }


    public int BlockSizeBytes { get; } = 8;

    public byte[] Encrypt(byte[] block)
    {
        var initialPermutation = _InitialPermutation(block);
        var encrypted = _fiestelCipher.DoFiestelCipher(initialPermutation);
        return _InitialPermutationReverse(encrypted);
    }

    public byte[] Decrypt(byte[] block)
    {
        var initialPermutation = _InitialPermutation(block);
        var decrypted = _fiestelCipher.DoFiestelCipher(initialPermutation, isEncrypt: false);
        return _InitialPermutationReverse(decrypted);
    }

    private byte[] _InitialPermutation(byte[] block)
    {
        byte[] pBlock =
        [
            58, 50, 42, 34, 26, 18, 10, 2,
            60, 52, 44, 36, 28, 20, 12, 4,
            62, 54, 46, 38, 30, 22, 14, 6,
            64, 56, 48, 40, 32, 24, 16, 8,
            57, 49, 41, 33, 25, 17, 9, 1,
            59, 51, 43, 35, 27, 19, 11, 3,
            61, 53, 45, 37, 29, 21, 13, 5,
            63, 55, 47, 39, 31, 23, 15, 7,
        ];

        return PBlock.Permutate(block, pBlock, IndexBitsRule.FromMsb1);
    }

    private byte[] _InitialPermutationReverse(byte[] block)
    {
        byte[] pBlock =
        [
            40, 8, 48, 16, 56, 24, 64, 32,
            39, 7, 47, 15, 55, 23, 63, 31,
            38, 6, 46, 14, 54, 22, 62, 30,
            37, 5, 45, 13, 53, 21, 61, 29,
            36, 4, 44, 12, 52, 20, 60, 28,
            35, 3, 43, 11, 51, 19, 59, 27,
            34, 2, 42, 10, 50, 18, 58, 26,
            33, 1, 41, 9, 49, 17, 57, 25
        ];

        return PBlock.Permutate(block, pBlock, IndexBitsRule.FromMsb1);
    }
}