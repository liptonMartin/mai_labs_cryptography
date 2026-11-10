package interfaces;

public interface EncryptionRound {
    public void DoEncrypt(byte[][] state, byte[][] roundKey);

    public void DoDecrypt(byte[][] state, byte[][] roundKey);
}
