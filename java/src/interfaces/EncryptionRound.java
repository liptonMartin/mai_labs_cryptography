package interfaces;

public interface EncryptionRound {
    public byte[][] DoEncrypt(byte[][] state, byte[][] roundKey);

    public byte[][] DoDecrypt(byte[][] state, byte[][] roundKey);
}
