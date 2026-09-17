namespace Core.Lab_1;

public interface IExpandKey
{
    public List<byte[]> GenerateRoundKeys(byte[] inputKey);
}

public interface IEncryptionStep
{
    public byte[] Encrypt(byte[] inputBlock, byte[] roundKey);
}

public interface ISymmetricalEncryptDecrypt
{
    public int BlockSizeBytes { get; }
    
    public byte[] Encrypt(byte[] block);

    public byte[] Decrypt(byte[] block);
}