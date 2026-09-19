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
        
        var blockSizeBytes = block.Length;
        var halfBlockSizeBytes = blockSizeBytes / 2;
            
        for (var round = 0; round < RoundCount; ++round)
        {
            var leftBlock = block[..halfBlockSizeBytes];
            var rightBlock = block[halfBlockSizeBytes..];

            var tmp = rightBlock;
            if (isEncrypt)
            {
                rightBlock = EncryptionRound.DoEncrypt(leftBlock, RoundKeys[round]);
                leftBlock = tmp;
            }
            else
            {
                rightBlock = leftBlock;
                leftBlock = EncryptionRound.DoEncrypt(tmp, RoundKeys[RoundCount - round]);
            }

            block = leftBlock.Concat(rightBlock).ToArray();
        }

        return block;
    }
}