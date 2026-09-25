namespace Core.Lab_1;

public class Deal128RoundKeyGenerator : IRoundKeysGenerator
{
    public List<byte[]> GenerateRoundKeys(byte[] key)
    {
        ValidateKey(key);
        var desKey = new byte[] { 1, 35, 69, 103, 137, 171, 205, 239 };
        var des = new Des(desKey);
        var leftPartKey = GetLeftPartKey(key);
        var rightPartKey = GetRightPartKey(key);
        var keyParts = new List<byte[]> { leftPartKey, rightPartKey };
        var prevRoundKey = new byte[8];
        List<byte[]> roundKeys = [];
        for (var i = 0; i < 6; ++i)
        {
            var index = i % 2;
            var constant = GetConstant(i);
            var beforeEncrypt = Helper.XorArrayOfBytes(keyParts[index], prevRoundKey, constant);
            var roundKey = des.Encrypt(beforeEncrypt);
            roundKeys.Add(roundKey);

            prevRoundKey = roundKey;
        }

        return roundKeys;
    }

    private static byte[] GetLeftPartKey(byte[] key)
    {
        var result = new byte[8];
        Array.Copy(key, 0, result, 0, 8);
        return result;
    }

    private static byte[] GetRightPartKey(byte[] key)
    {
        var result = new byte[8];
        Array.Copy(key, 8, result, 0, 8);
        return result;
    }

    private void ValidateKey(byte[] key)
    {
        if (key.Length < 16)
            throw new InvalidOperationException("DEAL_128 supports only 128 bit key");
    }

    private byte[] GetConstant(int numberRound)
    {
        byte constant = numberRound switch
        {
            2 => 1,
            3 => 2,
            4 => 4,
            5 => 8,
            _ => 0,
        };

        var result = new byte[8];
        result[0] = constant;
        return result;
    }
}

public class Deal128FiestelFunction : IEncryptionRound
{
    public byte[] DoEncrypt(byte[] halfBlock, byte[] roundKey)
    {
        var des = new Des(roundKey);
        return des.Encrypt(halfBlock);
    }
}

public class Deal128 : ISymmetricalEncryptDecrypt
{
    private readonly FiestelCipher _fiestelCipher;

    public Deal128(byte[] key)
    {
        _fiestelCipher = new FiestelCipher(new Deal128RoundKeyGenerator(), new Deal128FiestelFunction());
        _fiestelCipher.GenerateRoundKeys(key);
        _fiestelCipher.RoundCount = 6;
    }

    public int BlockSizeBytes { get; } = 16;

    public byte[] Encrypt(byte[] block) => _fiestelCipher.DoFiestelCipher(block);

    public byte[] Decrypt(byte[] block) => _fiestelCipher.DoFiestelCipher(block, isEncrypt: false);
}