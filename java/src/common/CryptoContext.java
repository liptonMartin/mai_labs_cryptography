package common;

import interfaces.SymmetricalAlgorithm;
import models.EncryptMode;
import models.PaddingMode;

import java.math.BigInteger;
import java.util.Arrays;
import java.util.concurrent.CompletableFuture;
import java.util.function.Function;
import java.io.BufferedInputStream;
import java.io.BufferedOutputStream;
import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.concurrent.CompletionException;

public class CryptoContext {
    private final byte[] key;
    private final SymmetricalAlgorithm algorithm;
    private final EncryptMode encryptMode;
    private final PaddingMode paddingMode;
    private final byte[] initializationVector;

    private final int blockSizeBytes;

    public CryptoContext(
            byte[] key,
            Function<byte[], SymmetricalAlgorithm> algorithmFactory,
            EncryptMode encryptMode,
            PaddingMode paddingMode,
            byte[] initializationVector
    ) {
        if (key == null || key.length == 0)
            throw new IllegalArgumentException("Key cannot be null or empty");

        if (algorithmFactory == null)
            throw new IllegalArgumentException("Algorithm factory cannot be null");

        if (encryptMode == null)
            throw new IllegalArgumentException("Encryption mode cannot be null");

        if (paddingMode == null)
            throw new IllegalArgumentException("Padding mode cannot be null");

        this.key = key.clone();
        this.encryptMode = encryptMode;
        this.paddingMode = paddingMode;
        this.initializationVector = initializationVector == null
                ? null
                : initializationVector.clone();

        this.algorithm = createAlgorithm(algorithmFactory);
        this.blockSizeBytes = this.algorithm.getBlockSizeBytes();

        if (blockSizeBytes <= 0 || blockSizeBytes > 255)
            throw new IllegalArgumentException("Invalid algorithm block size");

        if (encryptMode != EncryptMode.ECB) {
            if (this.initializationVector == null)
                throw new IllegalArgumentException("Initialization vector cannot be null");

            if (this.initializationVector.length != blockSizeBytes)
                throw new IllegalArgumentException(
                        "Initialization vector length must equal block size"
                );
        }
    }

    public CompletableFuture<byte[]> encryptAsync(byte[] data) {
        if (data == null)
            return CompletableFuture.failedFuture(
                    new IllegalArgumentException("Data cannot be null")
            );

        return CompletableFuture.supplyAsync(() -> encrypt(data));
    }

    public CompletableFuture<byte[]> decryptAsync(byte[] data) {
        if (data == null)
            return CompletableFuture.failedFuture(
                    new IllegalArgumentException("Data cannot be null")
            );

        return CompletableFuture.supplyAsync(() -> decrypt(data));
    }

    public CompletableFuture<Void> encryptFileAsync(
            Path inputPath,
            Path outputPath,
            int chunkSizeBytes
    ) {
        return CompletableFuture.runAsync(() -> {
            try {
                processFile(inputPath, outputPath, chunkSizeBytes, true);
            } catch (IOException e) {
                throw new CompletionException(e);
            }
        });
    }

    public CompletableFuture<Void> decryptFileAsync(
            Path inputPath,
            Path outputPath,
            int chunkSizeBytes
    ) {
        return CompletableFuture.runAsync(() -> {
            try {
                processFile(inputPath, outputPath, chunkSizeBytes, false);
            } catch (IOException e) {
                throw new CompletionException(e);
            }
        });
    }

    private void processFile(
            Path inputPath,
            Path outputPath,
            int chunkSizeBytes,
            boolean encrypt
    ) throws IOException {
        if (inputPath == null || outputPath == null)
            throw new IllegalArgumentException("File path cannot be null");

        if (chunkSizeBytes <= 0)
            throw new IllegalArgumentException("Chunk size must be positive");

        var processor = new StreamProcessor(
                algorithm,
                encryptMode,
                initializationVector
        );

        try (
                var input = new BufferedInputStream(
                        Files.newInputStream(inputPath),
                        chunkSizeBytes
                );
                var output = new BufferedOutputStream(
                        Files.newOutputStream(outputPath),
                        chunkSizeBytes
                )
        ) {
            if (encrypt) {
                processFileEncryption(input, output, processor);
            } else {
                processFileDecryption(input, output, processor);
            }
        }
    }

    private void processFileEncryption(
            BufferedInputStream input,
            BufferedOutputStream output,
            StreamProcessor processor
    ) throws IOException {
        while (true) {
            var block = input.readNBytes(blockSizeBytes);

            if (block.length == 0)
                break;

            if (block.length < blockSizeBytes) {
                var paddedBlock = Padding.addPadding(
                        block,
                        blockSizeBytes,
                        paddingMode
                );

                if (paddedBlock.length > 0) {
                    var encryptedBlock = processor.processBlock(paddedBlock, true);
                    output.write(encryptedBlock);
                }

                return;
            }

            var encryptedBlock = processor.processBlock(block, true);
            output.write(encryptedBlock);
        }

        var finalBlock = Padding.addPadding(
                new byte[0],
                blockSizeBytes,
                paddingMode
        );

        if (finalBlock.length > 0) {
            var encryptedBlock = processor.processBlock(finalBlock, true);
            output.write(encryptedBlock);
        }
    }

    private void processFileDecryption(
            BufferedInputStream input,
            BufferedOutputStream output,
            StreamProcessor processor
    ) throws IOException {
        byte[] lastDecryptedBlock = null;

        while (true) {
            var encryptedBlock = input.readNBytes(blockSizeBytes);

            if (encryptedBlock.length == 0)
                break;

            if (encryptedBlock.length != blockSizeBytes)
                throw new IllegalArgumentException("Invalid encrypted file length");

            if (lastDecryptedBlock != null)
                output.write(lastDecryptedBlock);

            lastDecryptedBlock = processor.processBlock(encryptedBlock, false);
        }

        if (lastDecryptedBlock == null) {
            if (paddingMode != PaddingMode.ZEROS)
                throw new IllegalArgumentException("Encrypted file is empty");

            return;
        }

        var unpaddedBlock = Padding.removePadding(
                lastDecryptedBlock,
                blockSizeBytes,
                paddingMode
        );

        output.write(unpaddedBlock);
    }

    private byte[] encrypt(byte[] data) {
        var paddedData = Padding.addPadding(data, blockSizeBytes, paddingMode);
        var countBlocks = paddedData.length / blockSizeBytes;

        if (countBlocks == 0)
            return new byte[0];

        return switch (encryptMode) {
            case ECB -> encryptEcb(paddedData, countBlocks);
            case CBC -> encryptCbc(paddedData, countBlocks);
            case PCBC -> encryptPcbc(paddedData, countBlocks);
            case CFB -> encryptCfb(paddedData, countBlocks);
            case OFB -> encryptOfb(paddedData, countBlocks);
            case CTR -> encryptCounterMode(paddedData, countBlocks, false);
            case RANDOM_DELTA -> encryptCounterMode(paddedData, countBlocks, true);
            default -> throw new UnsupportedOperationException(
                    "Encryption mode is not implemented yet: " + encryptMode
            );
        };
    }

    private byte[] decrypt(byte[] data) {
        if (data.length == 0 || data.length % blockSizeBytes != 0)
            throw new IllegalArgumentException("Invalid encrypted data length");

        var countBlocks = data.length / blockSizeBytes;

        var decryptedData = switch (encryptMode) {
            case ECB -> decryptEcb(data, countBlocks);
            case CBC -> decryptCbc(data, countBlocks);
            case PCBC -> decryptPcbc(data, countBlocks);
            case CFB -> decryptCfb(data, countBlocks);
            case OFB -> decryptOfb(data, countBlocks);
            case CTR -> encryptCounterMode(data, countBlocks, false);
            case RANDOM_DELTA -> encryptCounterMode(data, countBlocks, true);
            default -> throw new UnsupportedOperationException(
                    "Decryption mode is not implemented yet: " + encryptMode
            );
        };

        return Padding.removePadding(decryptedData, blockSizeBytes, paddingMode);
    }

    private byte[] encryptEcb(byte[] data, int countBlocks) {
        var result = new byte[data.length];

        var tasks = new CompletableFuture<?>[countBlocks];

        for (int i = 0; i < countBlocks; ++i) {
            var blockNumber = i;
            tasks[i] = CompletableFuture.runAsync(() -> {
                var block = getBlock(data, blockNumber);
                var encryptedBlock = algorithm.Encrypt(block);

                validateBlock(encryptedBlock);

                System.arraycopy(
                        encryptedBlock, 0,
                        result, blockNumber * blockSizeBytes,
                        blockSizeBytes
                );
            });
        }

        CompletableFuture.allOf(tasks).join();

        return result;
    }

    private byte[] decryptEcb(byte[] data, int countBlocks) {
        var result = new byte[data.length];

        var tasks = new CompletableFuture<?>[countBlocks];

        for (int i = 0; i < countBlocks; ++i) {
            var blockNumber = i;
            tasks[i] = CompletableFuture.runAsync(() -> {
                var block = getBlock(data, blockNumber);
                var decryptedBlock = algorithm.Decrypt(block);

                validateBlock(decryptedBlock);

                System.arraycopy(
                        decryptedBlock, 0,
                        result, blockNumber * blockSizeBytes,
                        blockSizeBytes
                );
            });
        }

        CompletableFuture.allOf(tasks).join();

        return result;
    }

    private byte[] encryptCbc(byte[] data, int countBlocks) {
        var result = new byte[data.length];

        var previousBlock = initializationVector.clone();

        for (int i = 0; i < countBlocks; ++i) {
            var block = getBlock(data, i);
            var gamma = xor(block, previousBlock);
            var encryptedBlock = algorithm.Encrypt(gamma);

            validateBlock(encryptedBlock);

            System.arraycopy(
                    encryptedBlock, 0,
                    result, i * blockSizeBytes,
                    blockSizeBytes
            );

            previousBlock = encryptedBlock;
        }

        return result;
    }

    private byte[] decryptCbc(byte[] data, int countBlocks) {
        var result = new byte[data.length];

        for (int i = 0; i < countBlocks; ++i) {
            var block = getBlock(data, i);

            var previousBlock = i == 0
                    ? initializationVector
                    : getBlock(data, i - 1);

            var decryptedBlock = algorithm.Decrypt(block);
            validateBlock(decryptedBlock);

            var plainBlock = xor(decryptedBlock, previousBlock);

            System.arraycopy(
                    plainBlock, 0,
                    result, i * blockSizeBytes,
                    blockSizeBytes
            );
        }

        return result;
    }

    private byte[] encryptPcbc(byte[] data, int countBlocks) {
        var result = new byte[data.length];

        var previousEncryptedBlock = initializationVector.clone();
        var previousDecryptedBlock = new byte[blockSizeBytes];

        for (int i = 0; i < countBlocks; ++i) {
            var block = getBlock(data, i);

            var gamma = xor(block, previousEncryptedBlock, previousDecryptedBlock);
            var encryptedBlock = algorithm.Encrypt(gamma);

            validateBlock(encryptedBlock);

            System.arraycopy(
                    encryptedBlock, 0,
                    result, i * blockSizeBytes,
                    blockSizeBytes
            );

            previousEncryptedBlock = encryptedBlock;
            previousDecryptedBlock = block;
        }

        return result;
    }

    private byte[] decryptPcbc(byte[] data, int countBlocks) {
        var result = new byte[data.length];

        var previousEncryptedBlock = initializationVector.clone();
        var previousDecryptedBlock = new byte[blockSizeBytes];

        for (int i = 0; i < countBlocks; ++i) {
            var block = getBlock(data, i);

            var decryptedBlock = algorithm.Decrypt(block);
            validateBlock(decryptedBlock);

            var plainBlock = xor(
                    decryptedBlock,
                    previousEncryptedBlock,
                    previousDecryptedBlock
            );

            System.arraycopy(
                    plainBlock, 0,
                    result, i * blockSizeBytes,
                    blockSizeBytes
            );

            previousEncryptedBlock = block;
            previousDecryptedBlock = plainBlock;
        }

        return result;
    }

    private byte[] encryptCfb(byte[] data, int countBlocks) {
        var result = new byte[data.length];

        var previousEncryptedBlock = initializationVector.clone();

        for (int i = 0; i < countBlocks; ++i) {
            var block = getBlock(data, i);

            var gamma = algorithm.Encrypt(previousEncryptedBlock);
            validateBlock(gamma);

            var encryptedBlock = xor(block, gamma);

            System.arraycopy(
                    encryptedBlock, 0,
                    result, i * blockSizeBytes,
                    blockSizeBytes
            );

            previousEncryptedBlock = encryptedBlock;
        }

        return result;
    }

    private byte[] decryptCfb(byte[] data, int countBlocks) {
        var result = new byte[data.length];

        var previousEncryptedBlock = initializationVector.clone();

        for (int i = 0; i < countBlocks; ++i) {
            var block = getBlock(data, i);

            var gamma = algorithm.Encrypt(previousEncryptedBlock);
            validateBlock(gamma);

            var decryptedBlock = xor(block, gamma);

            System.arraycopy(
                    decryptedBlock, 0,
                    result, i * blockSizeBytes,
                    blockSizeBytes
            );

            previousEncryptedBlock = block;
        }

        return result;
    }

    private byte[] encryptOfb(byte[] data, int countBlocks) {
        var result = new byte[data.length];

        var previousGamma = initializationVector.clone();

        for (int i = 0; i < countBlocks; ++i) {
            var block = getBlock(data, i);

            var gamma = algorithm.Encrypt(previousGamma);
            validateBlock(gamma);

            var encryptedBlock = xor(block, gamma);

            System.arraycopy(
                    encryptedBlock, 0,
                    result, i * blockSizeBytes,
                    blockSizeBytes
            );

            previousGamma = gamma;
        }

        return result;
    }

    private byte[] decryptOfb(byte[] data, int countBlocks) {
        var result = new byte[data.length];

        var previousGamma = initializationVector.clone();

        for (int i = 0; i < countBlocks; ++i) {
            var block = getBlock(data, i);

            var gamma = algorithm.Encrypt(previousGamma);
            validateBlock(gamma);

            var decryptedBlock = xor(block, gamma);

            System.arraycopy(
                    decryptedBlock, 0,
                    result, i * blockSizeBytes,
                    blockSizeBytes
            );

            previousGamma = gamma;
        }

        return result;
    }

    private byte[] encryptCounterMode(byte[] data, int countBlocks, boolean randomDelta) {
        var result = new byte[data.length];
        var initialCounter = new BigInteger(1, initializationVector);

        var delta = BigInteger.ONE;

        if (randomDelta) {
            var halfBlockBits = blockSizeBytes * 4;
            var mask = BigInteger.ONE.shiftLeft(halfBlockBits).subtract(BigInteger.ONE);

            delta = initialCounter.and(mask);

            if (!delta.testBit(0))
                delta = delta.add(BigInteger.ONE);
        }

        var counterLimit = BigInteger.ONE.shiftLeft(blockSizeBytes * 8);

        var tasks = new CompletableFuture<?>[countBlocks];

        for (int i = 0; i < countBlocks; ++i) {
            var blockIndex = i;
            var counter = initialCounter.add(delta.multiply(BigInteger.valueOf(blockIndex)));

            if (counter.compareTo(counterLimit) >= 0)
                throw new IllegalArgumentException("Counter overflow");

            var counterBlock = toBlockBytes(counter);

            tasks[i] = CompletableFuture.runAsync(() -> {
                var gamma = algorithm.Encrypt(counterBlock);
                validateBlock(gamma);

                var block = getBlock(data, blockIndex);
                var processedBlock = xor(block, gamma);

                System.arraycopy(
                        processedBlock,
                        0,
                        result,
                        blockIndex * blockSizeBytes,
                        blockSizeBytes
                );
            });
        }

        CompletableFuture.allOf(tasks).join();

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

    private SymmetricalAlgorithm createAlgorithm(Function<byte[], SymmetricalAlgorithm> algorithmFactory) {
        var algorithm = algorithmFactory.apply(key.clone());

        if (algorithm == null)
            throw new IllegalStateException("Algorithm factory returned null");

        if (algorithm.getBlockSizeBytes() != blockSizeBytes && blockSizeBytes != 0)
            throw new IllegalStateException("Algorithm block size has changed");

        return algorithm;
    }

    private byte[] getBlock(byte[] data, int blockNumber) {
        var offset = blockNumber * blockSizeBytes;

        return Arrays.copyOfRange(
                data,
                offset,
                offset + blockSizeBytes
        );
    }

    private byte[] xor(byte[]... blocks) {
        var result = new byte[blockSizeBytes];

        for (var block : blocks) {
            if (block.length != blockSizeBytes)
                throw new IllegalArgumentException("Blocks have different lengths");

            for (int i = 0; i < blockSizeBytes; ++i)
                result[i] ^= block[i];
        }

        return result;
    }

    private void validateBlock(byte[] block) {
        if (block == null || block.length != blockSizeBytes)
            throw new IllegalStateException(
                    "Algorithm returned a block with invalid length"
            );
    }
}
