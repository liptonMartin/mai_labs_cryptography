package aes;

import interfaces.KeyExpansion;
import models.Size;


public class RijndaelKeyExpansion implements KeyExpansion {
    private final GaloisField GF;
    private final RijndaelOperations RO;
    private final Size blockSize;
    private final Size keySize;

    private final int NB;
    private final int NK;

    private final int roundsCount;
    private final int totalWords;

    private final byte[][] rCon;


    public RijndaelKeyExpansion(GaloisField GF, RijndaelOperations RO, Size blockSize, Size keySize) {
        this.GF = GF;
        this.RO = RO;
        this.blockSize = blockSize;
        this.keySize = keySize;

        this.NB = blockSize.getValue() / 32;
        this.NK = keySize.getValue() / 32;

        this.roundsCount = defineRoundsCount(blockSize, keySize);
        this.totalWords = NB * (this.roundsCount + 1);

        this.rCon = generateRCon();
    }

    @Override
    public byte[][][] ExpandKey(byte[] key) {
        if (key.length * 8 != keySize.getValue())
            throw new IllegalArgumentException(
                    String.format(
                            "The secret key has invalid size %s, right size %s!", key.length * 8, keySize.getValue()
                    )
            );

        var words = new byte[totalWords][4];
        for (int i = 0; i < NK; ++i) {
            System.arraycopy(key, i * 4, words[i], 0, 4);
        }

        for (int i = NK; i < totalWords; ++i) {
            byte[] temp = words[i - 1].clone();
            if (i % NK == 0) {
                rotWord(temp);
                RO.subWord(temp);
                GF.add(temp, rCon[(i / NK) - 1]);
            } else if (NK > 6 && i % NK == 4) {
                RO.subWord(temp);
            }
            words[i] = GF.add(temp, words[i - NK]);
        }

        var roundKeys = new byte[roundsCount + 1][4][NB];
        for (int round = 0; round < roundsCount + 1; ++round) {
            for (int column = 0; column < NB; ++column) {
                for (int i = 0; i < 4; ++i) {
                    roundKeys[round][i][column] = words[round * NB + column][i];
                }
            }
        }

        return roundKeys;
    }

    private byte[][] generateRCon() {
        var RConLength = (totalWords - 1) / NK;

        var result = new byte[RConLength][4];
        result[0] = new byte[]{1, 0, 0, 0};

        for (int i = 1; i < RConLength; ++i) {
            result[i] = GF.multiply(result[i - 1].clone(), new byte[]{2, 2, 2, 2});
        }

        return result;
    }

    private void rotWord(byte[] word) {
        var tmp = word[0];
        word[0] = word[1];
        word[1] = word[2];
        word[2] = word[3];
        word[3] = tmp;
    }

    private int defineRoundsCount(Size blockSize, Size keySize) {
        if (blockSize == Size.S256 || keySize == Size.S256)
            return 14;
        if (blockSize == Size.S192 || keySize == Size.S192)
            return 12;
        return 10;
    }
}
