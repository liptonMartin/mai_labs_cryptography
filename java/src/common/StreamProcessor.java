package common;

import interfaces.SymmetricalAlgorithm;
import models.EncryptMode;

import java.math.BigInteger;

public class StreamProcessor {
    private final SymmetricalAlgorithm algorithm;
    private final EncryptMode encryptMode;
    private final int blockSizeBytes;

    private byte[] previousBlock;
    private byte[] previousGamma;

    private BigInteger counter;
    private BigInteger delta;
    private final BigInteger counterLimit;

    public StreamProcessor(
            SymmetricalAlgorithm algorithm,
            EncryptMode encryptMode,
            byte[] initializationVector
    ) {
        if (algorithm == null)
            throw new IllegalArgumentException("Algorithm cannot be null");

        if (encryptMode == null)
            throw new IllegalArgumentException("Encryption mode cannot be null");

        this.algorithm = algorithm;
        this.encryptMode = encryptMode;
        this.blockSizeBytes = algorithm.getBlockSizeBytes();

        if (blockSizeBytes <= 0)
            throw new IllegalArgumentException("Invalid block size");

        if (encryptMode != EncryptMode.ECB) {
            if (initializationVector == null ||
                    initializationVector.length != blockSizeBytes)
                throw new IllegalArgumentException("Invalid initialization vector");
        }

        this.previousBlock = initializationVector == null
                ? null
                : initializationVector.clone();

        this.previousGamma = initializationVector == null
                ? null
                : initializationVector.clone();

        this.counterLimit = BigInteger.ONE.shiftLeft(blockSizeBytes * 8);

        if (encryptMode == EncryptMode.CTR ||
                encryptMode == EncryptMode.RANDOM_DELTA) {

            this.counter = new BigInteger(1, initializationVector);

            if (encryptMode == EncryptMode.RANDOM_DELTA) {
                var halfBlockBits = blockSizeBytes * 4;
                var mask = BigInteger.ONE.shiftLeft(halfBlockBits)
                        .subtract(BigInteger.ONE);

                this.delta = counter.and(mask);

                if (!this.delta.testBit(0))
                    this.delta = this.delta.add(BigInteger.ONE);
            } else {
                this.delta = BigInteger.ONE;
            }
        } else {
            this.counter = null;
            this.delta = null;
        }
    }

    public byte[] processBlock(byte[] block, boolean encrypt) {
        if (block == null || block.length != blockSizeBytes)
            throw new IllegalArgumentException("Invalid block");

        var result = switch (encryptMode) {
            case ECB -> encrypt
                    ? algorithm.Encrypt(block)
                    : algorithm.Decrypt(block);

            case CBC -> processCbc(block, encrypt);
            case PCBC -> processPcbc(block, encrypt);
            case CFB -> processCfb(block, encrypt);
            case OFB -> processOfb(block);
            case CTR, RANDOM_DELTA -> processCounter(block);
        };

        if (result == null || result.length != blockSizeBytes)
            throw new IllegalArgumentException("Algorithm returned invalid block");

        return result;
    }

    private byte[] processCbc(byte[] block, boolean encrypt) {
        if (encrypt) {
            var result = algorithm.Encrypt(xor(block, previousBlock));
            previousBlock = result.clone();
            return result;
        }

        var decryptedBlock = algorithm.Decrypt(block);
        var result = xor(decryptedBlock, previousBlock);

        previousBlock = block.clone();

        return result;
    }

    private byte[] processPcbc(byte[] block, boolean encrypt) {
        if (encrypt) {
            var result = algorithm.Encrypt(xor(block, previousBlock));
            previousBlock = xor(block, result);
            return result;
        }

        var decryptedBlock = algorithm.Decrypt(block);
        var result = xor(decryptedBlock, previousBlock);

        previousBlock = xor(result, block);

        return result;
    }

    private byte[] processCfb(byte[] block, boolean encrypt) {
        var gamma = algorithm.Encrypt(previousBlock);
        var result = xor(block, gamma);

        previousBlock = encrypt ? result.clone() : block.clone();

        return result;
    }

    private byte[] processOfb(byte[] block) {
        previousGamma = algorithm.Encrypt(previousGamma);
        return xor(block, previousGamma);
    }

    private byte[] processCounter(byte[] block) {
        if (counter.compareTo(counterLimit) >= 0)
            throw new IllegalArgumentException("Counter overflow");

        var counterBlock = toBlockBytes(counter);
        var gamma = algorithm.Encrypt(counterBlock);

        counter = counter.add(delta);

        return xor(block, gamma);
    }

    private byte[] xor(byte[] first, byte[] second) {
        var result = new byte[blockSizeBytes];

        for (int i = 0; i < blockSizeBytes; ++i)
            result[i] = (byte) (first[i] ^ second[i]);

        return result;
    }

    private byte[] toBlockBytes(BigInteger value) {
        var encoded = value.toByteArray();
        var result = new byte[blockSizeBytes];

        var sourceStart = Math.max(0, encoded.length - blockSizeBytes);
        var length = encoded.length - sourceStart;

        System.arraycopy(
                encoded,
                sourceStart,
                result,
                blockSizeBytes - length,
                length
        );

        return result;
    }
}