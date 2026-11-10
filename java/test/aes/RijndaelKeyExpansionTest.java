package aes;

import models.Size;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.params.ParameterizedTest;
import org.junit.jupiter.params.provider.CsvSource;

import java.lang.reflect.Method;

import static org.junit.jupiter.api.Assertions.*;

public class RijndaelKeyExpansionTest {

    private GaloisField gf;
    private RijndaelOperations rijndael;

    @BeforeEach
    void setUp() {
        gf = new GaloisField();
        rijndael = new RijndaelOperations(gf, Size.S128);
    }

    // ------------------------------------------------------------------
    // helpers
    // ------------------------------------------------------------------

    private RijndaelKeyExpansion create(Size blockSize, Size keySize) {
        return new RijndaelKeyExpansion(gf, rijndael, blockSize, keySize);
    }

    // слова (int, hex) -> байты ключа: key(0x2b7e1516, 0x28aed2a6, ...)
    private static byte[] key(int... words) {
        byte[] result = new byte[words.length * 4];
        for (int w = 0; w < words.length; w++)
            for (int i = 0; i < 4; i++)
                result[w * 4 + i] = (byte) (words[w] >>> (24 - 8 * i));
        return result;
    }

    // слова (int, hex) -> матрица 4 x NB, слово лежит столбцом, как в состоянии
    private static byte[][] roundKey(int... words) {
        byte[][] result = new byte[4][words.length];
        for (int c = 0; c < words.length; c++)
            for (int r = 0; r < 4; r++)
                result[r][c] = (byte) (words[c] >>> (24 - 8 * r));
        return result;
    }

    private static byte[] row(int... values) {
        byte[] result = new byte[values.length];
        for (int i = 0; i < values.length; i++)
            result[i] = (byte) values[i];
        return result;
    }

    private static Object invokePrivate(
            RijndaelKeyExpansion expansion,
            String methodName,
            Class<?>[] types,
            Object... args
    ) throws Exception {
        Method method = RijndaelKeyExpansion.class.getDeclaredMethod(methodName, types);
        method.setAccessible(true);
        return method.invoke(expansion, args);
    }

    // ------------------------------------------------------------------
    // ExpandKey: shape and errors
    // ------------------------------------------------------------------

    @ParameterizedTest(name = "block = {0}, key = {1}: {2} round keys of 4x{3}")
    @CsvSource({
            "S128, S128, 11, 4",
            "S128, S192, 13, 4",
            "S128, S256, 15, 4",
            "S192, S128, 13, 6",
            "S256, S256, 15, 8",
    })
    @DisplayName("ExpandKey returns (rounds + 1) matrices 4 x Nb")
    void expandKeyShapeTest(Size blockSize, Size keySize, int roundKeysCount, int columns) {
        byte[][][] roundKeys = create(blockSize, keySize).ExpandKey(new byte[keySize.getValue() / 8]);

        assertEquals(roundKeysCount, roundKeys.length);
        for (byte[][] roundKey : roundKeys) {
            assertEquals(4, roundKey.length);
            for (byte[] r : roundKey)
                assertEquals(columns, r.length);
        }
    }

    @Test
    @DisplayName("ExpandKey throws on invalid key size")
    void expandKeyInvalidKeySizeTest() {
        RijndaelKeyExpansion expansion = create(Size.S128, Size.S128);
        assertThrows(IllegalArgumentException.class, () -> expansion.ExpandKey(new byte[24]));
    }

    // ------------------------------------------------------------------
    // ExpandKey: FIPS-197 Appendix A
    // ------------------------------------------------------------------

    @Test
    @DisplayName("AES-128 key expansion matches FIPS-197 Appendix A.1")
    void expandKeyAes128Test() {
        byte[][][] roundKeys = create(Size.S128, Size.S128)
                .ExpandKey(key(0x2b7e1516, 0x28aed2a6, 0xabf71588, 0x09cf4f3c));

        assertArrayEquals(roundKey(0x2b7e1516, 0x28aed2a6, 0xabf71588, 0x09cf4f3c), roundKeys[0]);
        assertArrayEquals(roundKey(0xa0fafe17, 0x88542cb1, 0x23a33939, 0x2a6c7605), roundKeys[1]);
        assertArrayEquals(roundKey(0xf2c295f2, 0x7a96b943, 0x5935807a, 0x7359f67f), roundKeys[2]);
        assertArrayEquals(roundKey(0xd014f9a8, 0xc9ee2589, 0xe13f0cc8, 0xb6630ca6), roundKeys[10]);
    }

    @Test
    @DisplayName("AES-192 key expansion matches FIPS-197 Appendix A.2")
    void expandKeyAes192Test() {
        byte[][][] roundKeys = create(Size.S128, Size.S192)
                .ExpandKey(key(0x8e73b0f7, 0xda0e6452, 0xc810f32b, 0x809079e5, 0x62f8ead2, 0x522c6b7b));

        assertArrayEquals(roundKey(0x8e73b0f7, 0xda0e6452, 0xc810f32b, 0x809079e5), roundKeys[0]);
        assertArrayEquals(roundKey(0x62f8ead2, 0x522c6b7b, 0xfe0c91f7, 0x2402f5a5), roundKeys[1]);
        assertArrayEquals(roundKey(0xe98ba06f, 0x448c773c, 0x8ecc7204, 0x01002202), roundKeys[12]);
    }

    @Test
    @DisplayName("AES-256 key expansion matches FIPS-197 Appendix A.3")
    void expandKeyAes256Test() {
        byte[][][] roundKeys = create(Size.S128, Size.S256).ExpandKey(key(
                0x603deb10, 0x15ca71be, 0x2b73aef0, 0x857d7781,
                0x1f352c07, 0x3b6108d7, 0x2d9810a3, 0x0914dff4));

        assertArrayEquals(roundKey(0x603deb10, 0x15ca71be, 0x2b73aef0, 0x857d7781), roundKeys[0]);
        assertArrayEquals(roundKey(0x1f352c07, 0x3b6108d7, 0x2d9810a3, 0x0914dff4), roundKeys[1]);
        assertArrayEquals(roundKey(0x9ba35411, 0x8e6925af, 0xa51a8b5f, 0x2067fcde), roundKeys[2]);
        assertArrayEquals(roundKey(0xfe4890d1, 0xe6188d0b, 0x046df344, 0x706c631e), roundKeys[14]);
    }

    @Test
    @DisplayName("ExpandKey does not modify the secret key")
    void expandKeyKeyIsNotModifiedTest() {
        byte[] secret = key(0x2b7e1516, 0x28aed2a6, 0xabf71588, 0x09cf4f3c);
        byte[] expected = secret.clone();

        create(Size.S128, Size.S128).ExpandKey(secret);

        assertArrayEquals(expected, secret);
    }

    // ------------------------------------------------------------------
    // defineRoundsCount
    // ------------------------------------------------------------------

    @ParameterizedTest(name = "block = {0}, key = {1} -> {2} rounds")
    @CsvSource({
            "S128, S128, 10",
            "S192, S128, 12",
            "S128, S192, 12",
            "S192, S192, 12",
            "S256, S128, 14",
            "S128, S256, 14",
            "S192, S256, 14",
            "S256, S256, 14",
    })
    @DisplayName("defineRoundsCount = max(Nb, Nk) + 6")
    void defineRoundsCountTest(Size blockSize, Size keySize, int expected) throws Exception {
        RijndaelKeyExpansion expansion = create(Size.S128, Size.S128);
        Object actual = invokePrivate(
                expansion, "defineRoundsCount", new Class<?>[]{Size.class, Size.class}, blockSize, keySize);
        assertEquals(expected, actual);
    }

    // ------------------------------------------------------------------
    // generateRCon / rotWord
    // ------------------------------------------------------------------

    @Test
    @DisplayName("generateRCon matches FIPS-197 Rcon values for AES-128")
    void generateRConValuesTest() throws Exception {
        byte[][] rCon = (byte[][]) invokePrivate(
                create(Size.S128, Size.S128), "generateRCon", new Class<?>[]{});

        int[] expected = {0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80, 0x1B, 0x36};
        assertEquals(expected.length, rCon.length);
        for (int i = 0; i < expected.length; i++)
            assertArrayEquals(row(expected[i], 0, 0, 0), rCon[i], "Rcon[" + (i + 1) + "]");
    }

    @ParameterizedTest(name = "block = {0}, key = {1} -> {2} Rcon values")
    @CsvSource({
            "S128, S128, 10",
            "S128, S192, 8",
            "S128, S256, 7",
    })
    @DisplayName("generateRCon length depends on key size")
    void generateRConLengthTest(Size blockSize, Size keySize, int expected) throws Exception {
        byte[][] rCon = (byte[][]) invokePrivate(
                create(blockSize, keySize), "generateRCon", new Class<?>[]{});
        assertEquals(expected, rCon.length);
    }

    @Test
    @DisplayName("rotWord rotates the word one byte to the left")
    void rotWordTest() throws Exception {
        byte[] actual = (byte[]) invokePrivate(
                create(Size.S128, Size.S128), "rotWord", new Class<?>[]{byte[].class}, (Object) row(1, 2, 3, 4));
        assertArrayEquals(row(2, 3, 4, 1), actual);
    }
}