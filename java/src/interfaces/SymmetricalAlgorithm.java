package interfaces;

public interface SymmetricalAlgorithm {
    public int getBlockSizeBytes();

    public byte[] Encrypt(byte[] block);

    public byte[] Decrypt(byte[] block);
}
