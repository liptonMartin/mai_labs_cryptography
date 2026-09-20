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
                var fiestelFunctionResult = EncryptionRound.DoEncrypt(rightBlock, RoundKeys[round]);
                rightBlock = Helper.XorArrayOfBytes(leftBlock, fiestelFunctionResult);
                leftBlock = tmp;
            }
            else
            {
                rightBlock = leftBlock;
                var fiestelFunctionResult = EncryptionRound.DoEncrypt(leftBlock, RoundKeys[RoundCount - round - 1]);
                leftBlock = Helper.XorArrayOfBytes(tmp, fiestelFunctionResult);
            }

            block = leftBlock.Concat(rightBlock).ToArray();
        }
        var endLeftBlock = block[..halfBlockSizeBytes];
        var endRightBlock = block[halfBlockSizeBytes..];
        
        block = endRightBlock.Concat(endLeftBlock).ToArray();

        return block;
    }
}