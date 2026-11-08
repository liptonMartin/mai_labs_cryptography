package aes;

import common.Helper;
import interfaces.SymmetricalAlgorithm;
import models.Size;

import java.util.Arrays;
import java.util.HashMap;
import java.util.List;
import java.util.Map;


public class Rijndael implements SymmetricalAlgorithm {
    private final GaloisField GF = new GaloisField();

    private Size blockSize = Size.S128;
    private Size keySize = Size.S128;

    private final int roundsCount;

    public Rijndael(Size blockSize, Size keySize) {
        this.blockSize = blockSize;
        this.keySize = keySize;

        this.roundsCount = defineRoundsCount(blockSize, keySize);
    }

    @Override
    public List<Byte> Encrypt(List<Byte> block) {
        return List.of();
    }

    @Override
    public List<Byte> Decrypt(List<Byte> block) {
        return List.of();
    }

    private int defineRoundsCount(Size blockSize, Size keySize) {
        if (blockSize == Size.S256 || keySize == Size.S256)
            return 14;
        if (blockSize == Size.S192 || keySize == Size.S192)
            return 12;
        return 10;
    }
}
