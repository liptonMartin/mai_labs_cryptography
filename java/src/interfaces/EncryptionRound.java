package interfaces;

import java.util.List;

public interface EncryptionRound {
    public List<Byte> DoEncrypt(List<Byte> block, List<Byte> roundKey);
}
