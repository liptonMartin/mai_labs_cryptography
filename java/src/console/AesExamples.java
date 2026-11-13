package console;

import aes.RijndaelConfig;
import common.CryptoContext;
import models.EncryptMode;
import models.PaddingMode;
import models.Size;

import java.nio.charset.StandardCharsets;
import java.nio.file.Path;
import java.util.Arrays;

public final class AesExamples {

    private static final int FileChunkSizeBytes = 8192;

    private static final String DefaultMessage = "Hello, AES!";
    private static final EncryptMode DefaultEncryptMode = EncryptMode.ECB;
    private static final PaddingMode DefaultPaddingMode = PaddingMode.ZEROS;
    private static final Size DefaultBlockSize = Size.S128;
    private static final Size DefaultKeySize = Size.S128;

    private AesExamples() {
    }

    public static void aesMessage() throws Exception {
        Size blockSize = OutputHelper.readEnum("Input block size (bits)", DefaultBlockSize);
        int blockBytes = blockSize.getValue() / 8;

        Size keySize = OutputHelper.readEnum("Input key size (bits)", DefaultKeySize);
        int keyBytes = keySize.getValue() / 8;

        var defaultKey = new byte[keyBytes];
        Arrays.fill(defaultKey, (byte) 1);
        byte[] key = OutputHelper.readBytes(keyBytes, defaultKey, "Input key");

        String message = OutputHelper.readString("Input message", DefaultMessage);
        EncryptMode em = OutputHelper.readEnum("Input encryption mode", DefaultEncryptMode);
        PaddingMode pm = OutputHelper.readEnum("Input padding mode", DefaultPaddingMode);
        Byte mod = OutputHelper.readOptionalByte("Input Rijndael mod", "x8 + x4 + x3 + x + 1");

        byte[] iv = null;
        if (em != EncryptMode.ECB) {
            iv = OutputHelper.readBytes(blockBytes, new byte[blockBytes], "Input initialization vector");
        }

        var context = new CryptoContext(
                buildConfig(key, keySize, blockSize, mod), em, pm, iv);

        byte[] messageBytes = message.getBytes(StandardCharsets.UTF_8);
        byte[] encrypted = context.encryptAsync(messageBytes).join();
        byte[] decrypted = context.decryptAsync(encrypted).join();

        OutputHelper.outputByteArray(messageBytes, "Message");
        OutputHelper.outputByteArray(encrypted, "Encrypted message");
        OutputHelper.outputByteArray(decrypted, "Decrypted message");
        System.out.println("Decrypted text: " + new String(decrypted, StandardCharsets.UTF_8));
    }

    public static void aesFiles() throws Exception {
        Size blockSize = OutputHelper.readEnum("Input block size (bits)", DefaultBlockSize);
        int blockBytes = blockSize.getValue() / 8;

        Size keySize = OutputHelper.readEnum("Input key size (bits)", DefaultKeySize);
        int keyBytes = keySize.getValue() / 8;

        var defaultKey = new byte[keyBytes];
        Arrays.fill(defaultKey, (byte) 1);
        byte[] key = OutputHelper.readBytes(keyBytes, defaultKey, "Input key");

        EncryptMode em = OutputHelper.readEnum("Input encryption mode", DefaultEncryptMode);
        PaddingMode pm = OutputHelper.readEnum("Input padding mode", DefaultPaddingMode);
        Byte mod = OutputHelper.readOptionalByte("Input Rijndael mod", "x8 + x4 + x3 + x + 1");

        byte[] iv = null;
        if (em != EncryptMode.ECB) {
            iv = OutputHelper.readBytes(blockBytes, new byte[blockBytes], "Input initialization vector");
        }

        var context = new CryptoContext(
                buildConfig(key, keySize, blockSize, mod), em, pm, iv);

        Path input = OutputHelper.readFilePath("Input your file:");
        Path encrypted = OutputHelper.readFilePath("Input your output encrypted message file:");
        Path decrypted = OutputHelper.readFilePath("Input your output decrypted message file:");

        context.encryptFileAsync(input, encrypted, FileChunkSizeBytes).join();
        context.decryptFileAsync(encrypted, decrypted, FileChunkSizeBytes).join();
        System.out.println("Done!");
    }

    private static RijndaelConfig buildConfig(byte[] key, Size keySize, Size blockSize, Byte mod) {
        return mod == null
                ? new RijndaelConfig(key, blockSize)
                : new RijndaelConfig(key, blockSize, mod);
    }
}