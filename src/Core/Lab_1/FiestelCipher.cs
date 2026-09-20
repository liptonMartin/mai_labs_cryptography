namespace Core.Lab_1;

public class FiestelCipher(IRoundKeysGenerator roundKeysGenerator, IEncryptionRound encryptionRound)
{
    private IRoundKeysGenerator RoundKeysGenerator => roundKeysGenerator;
    private IEncryptionRound EncryptionRound => encryptionRound;

    public int RoundCount { get; set; } = 16;

    private List<byte[]>? RoundKeys { get; set; }

    public void GenerateRoundKeys(byte[] key)
    {
        if (RoundKeys is not null) return;

        RoundKeys = RoundKeysGenerator.GenerateRoundKeys(key);
    }

    public byte[] DoFiestelCipher(byte[] block, bool isEncrypt = true)
    {
        if (RoundKeys is null)
            throw new InvalidOperationException("Use GenerateRoundKeys method before DoFiestelCipher");

        var halfBlockSizeBytes = block.Length / 2;

        var left = block[..halfBlockSizeBytes];
        var right = block[halfBlockSizeBytes..];

        for (var round = 0; round < RoundCount; round++)
        {
            var keyIndex = isEncrypt ? round : RoundCount - 1 - round;

            var f = EncryptionRound.DoEncrypt(right, RoundKeys[keyIndex]);

            var newLeft = right;
            var newRight = Helper.XorArrayOfBytes(left, f);

            left = newLeft;
            right = newRight;
        }

        return right.Concat(left).ToArray();
    }
}