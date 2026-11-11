package aes;

import interfaces.EncryptionRound;
import interfaces.KeyExpansion;
import interfaces.SymmetricalAlgorithm;
import models.Size;


public class Rijndael implements SymmetricalAlgorithm {
    private final GaloisField GF;

    private RijndaelOperations RO;
    private EncryptionRound spNet;
    private KeyExpansion keyExpansion;

    private byte[] key;

    private Size blockSize;
    private Size keySize;

    private int roundsCount;

    private byte[][][] roundKeys;

    public Rijndael(byte[] key, Size blockSize) {
        this.GF = new GaloisField();
        this.setup(key, blockSize);
        this.generateRoundKeys();
    }

    public Rijndael(byte[] key, Size blockSize, byte GaloisFieldMod) {
        this.GF = new GaloisField(GaloisFieldMod);
        this.setup(key, blockSize);
        this.generateRoundKeys();
    }

    private void setup(byte[] key, Size blockSize) {
        this.key = key;
        this.blockSize = blockSize;

        var keyBits = key.length * 8;
        this.keySize = Size.valueOf(keyBits);

        this.roundsCount = defineRoundsCount(blockSize, keySize);

        this.RO = new RijndaelOperations(this.GF, blockSize);
        this.spNet = new SpNet(this.RO);
        this.keyExpansion = new RijndaelKeyExpansion(this.GF, this.RO, this.blockSize, this.keySize);
    }

    private void generateRoundKeys() {
        this.roundKeys = this.keyExpansion.ExpandKey(this.key);
    }

    @Override
    public int getBlockSizeBytes() {
        return this.blockSize.getValue() / 8;
    }

    @Override
    public byte[] Encrypt(byte[] block) {
        var state = convertBlockToState(block);

        RO.addRoundKey(state, roundKeys[0]);
        for (int i = 1; i < roundsCount; ++i) {
            spNet.DoEncrypt(state, roundKeys[i]);
        }

        RO.subBytes(state);
        RO.shiftRows(state);
        RO.addRoundKey(state, roundKeys[roundsCount]);

        block = convertStateToBlock(state);
        return block;
    }

    @Override
    public byte[] Decrypt(byte[] block) {
        var state = convertBlockToState(block);

        RO.addRoundKey(state, roundKeys[roundsCount]);
        for (int i = roundsCount - 1; i >= 1; --i) {
            spNet.DoDecrypt(state, roundKeys[i]);
        }

        RO.inverseShiftRows(state);
        RO.inverseSubBytes(state);
        RO.addRoundKey(state, roundKeys[0]);

        block = convertStateToBlock(state);
        return block;
    }

    private byte[][] convertBlockToState(byte[] block) {
        var NB = blockSize.getValue() / 32;
        var state = new byte[4][NB];

        for (int column = 0; column < NB; ++column) {
            for (int row = 0; row < 4; ++row) {
                state[row][column] = block[column * 4 + row];
            }
        }

        return state;
    }

    private byte[] convertStateToBlock(byte[][] state) {
        var NB = state[0].length;
        var block = new byte[NB * 4];

        for (int column = 0; column < NB; ++column) {
            for (int row = 0; row < 4; ++row) {
                block[column * 4 + row] = state[row][column];
            }
        }

        return block;
    }

    private int defineRoundsCount(Size blockSize, Size keySize) {
        if (blockSize == Size.S256 || keySize == Size.S256)
            return 14;
        if (blockSize == Size.S192 || keySize == Size.S192)
            return 12;
        return 10;
    }
}
