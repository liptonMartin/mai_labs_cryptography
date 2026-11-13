package aes;

import models.Config;
import models.Size;

public final class RijndaelConfig implements Config {
    private final byte[] key;
    private final Size blockSize;
    private Byte mod;

    public RijndaelConfig(byte[] key, Size blockSize) {
        this.key = key;
        this.blockSize = blockSize;
    }

    public RijndaelConfig(byte[] key, Size blockSize, byte mod) {
        this(key, blockSize);
        this.mod = mod;
    }

    @Override
    public byte[] getKey() {
        return key.clone();
    }

    @Override
    public Rijndael createAlgorithm() {
        if (mod == null)
            return new Rijndael(key, blockSize);
        return new Rijndael(key, blockSize, mod);
    }
}
