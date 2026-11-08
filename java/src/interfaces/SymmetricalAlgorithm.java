package interfaces;

import java.util.List;

public interface SymmetricalAlgorithm {
    public List<Byte> Encrypt(List<Byte> block);

    public List<Byte> Decrypt(List<Byte> block);
}
