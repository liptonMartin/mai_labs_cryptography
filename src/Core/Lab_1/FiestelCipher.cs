namespace Core.Lab_1;

public class FiestelCipher(IKeyExpander keyExpander, IEncryptionRound encryptionRound)
{
    private IKeyExpander KeyExpander => keyExpander;
    private IEncryptionRound EncryptionRound => encryptionRound;

    public int RoundCount { get; set; } = 16;

    private List<byte[]>? RoundKeys { get; set; }

    public void GenerateRoundKeys(byte[] key)
    {
        if (RoundKeys is not null) return;

        RoundKeys = KeyExpander.GenerateRoundKeys(key);
    }

    public byte[] DoFiestelCipher(byte[] block)
    {
        if (RoundKeys is null)
            throw new InvalidOperationException("Use GenerateRoundKeys method before DoFiestelCipher");
        
        var blockSizeBytes = block.Length;
        var halfBlockSizeBytes = blockSizeBytes / 2;
            
        for (var round = 0; round < RoundCount; ++round)
        {
            var leftBlock = block[..halfBlockSizeBytes];
            var rightBlock = block[halfBlockSizeBytes..];
            
            leftBlock = EncryptionRound.DoEncrypt(leftBlock, RoundKeys[round]);

            block = rightBlock.Concat(leftBlock).ToArray();
        }

        return block;
    }
}