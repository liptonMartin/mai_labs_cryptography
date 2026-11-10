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
        for (int i = 1; i < roundsCount - 1; ++i) {
            spNet.DoEncrypt(state, roundKeys[i]);
        }

        RO.subBytes(state);
        RO.shiftRows(state);
        RO.addRoundKey(state, roundKeys[roundsCount - 1]);

        block = convertStateToBlock(state);
        return block;
    }

    @Override
    public byte[] Decrypt(byte[] block) {
        var state = convertBlockToState(block);

        RO.addRoundKey(state, roundKeys[roundsCount - 1]);
        for (int i = roundsCount - 2; i >= 1; --i) {
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
        var wordSize = 4;
        var state = new byte[wordSize][NB];

        for (int i = 0; i < NB; ++i) {
            System.arraycopy(block, i * wordSize, state[i], 0, wordSize);
        }

        return state;
    }

    private byte[] convertStateToBlock(byte[][] state) {
        var wordSize = state.length;
        var NB = state[0].length;
        var length = NB * wordSize;
        var block = new byte[length];

        for (int i = 0; i < length; ++i) {
            block[i] = state[i % wordSize][i / NB];
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
