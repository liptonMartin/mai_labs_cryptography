namespace Core.Lab_1;

public interface IRoundKeysGenerator
{
    public List<byte[]> GenerateRoundKeys(byte[] key);
}

public interface IEncryptionRound
{
    public byte[] DoEncrypt(byte[] block, byte[] roundKey);
}

public interface ISymmetricalEncryptDecrypt
{
    public int BlockSizeBytes { get; }

    public byte[] Encrypt(byte[] block);

    public byte[] Decrypt(byte[] block);
}