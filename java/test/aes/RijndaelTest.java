package aes;

import models.Size;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.params.ParameterizedTest;
import org.junit.jupiter.params.provider.CsvSource;

import static org.junit.jupiter.api.Assertions.*;

public class RijndaelTest {

    // ------------------------------------------------------------------
    // helpers
    // ------------------------------------------------------------------

    private static byte[] hex(String value) {
        int length = value.length();
        byte[] result = new byte[length / 2];

        for (int i = 0; i < length; i += 2) {
            result[i / 2] = (byte) Integer.parseInt(
                    value.substring(i, i + 2), 16
            );
        }

        return result;
    }

    private static Rijndael createRijndael(String key) {
        return new Rijndael(hex(key), Size.S128);
    }

    // ------------------------------------------------------------------
    // getBlockSizeBytes
    // ------------------------------------------------------------------

    @ParameterizedTest(name = "Block size {0} bits = {1} bytes")
    @CsvSource({
            "128, 16",
            "192, 24",
            "256, 32"
    })
    @DisplayName("getBlockSizeBytes returns correct block size")
    void getBlockSizeBytesTest(int bits, int expected) {
        Rijndael rijndael = new Rijndael(
                hex("000102030405060708090A0B0C0D0E0F"),
                Size.valueOf(bits)
        );

        assertEquals(expected, rijndael.getBlockSizeBytes());
    }

    // ------------------------------------------------------------------
    // Encrypt
    // ------------------------------------------------------------------

    @ParameterizedTest(name = "AES encryption with {0}-bit key")
    @CsvSource({
            // AES-128
            "000102030405060708090A0B0C0D0E0F, 69C4E0D86A7B0430D8CDB78070B4C55A",

            // AES-192
            "000102030405060708090A0B0C0D0E0F1011121314151617, DDA97CA4864CDFE06EAF70A0EC0D7191",

            // AES-256
            "000102030405060708090A0B0C0D0E0F101112131415161718191A1B1C1D1E1F, 8EA2B7CA516745BFEAF C49904B496089"
    })
    @DisplayName("Encrypt matches FIPS-197 test vectors")
    void encryptFipsTest(String key, String expected) {
        Rijndael rijndael = createRijndael(key);

        byte[] plaintext = hex("00112233445566778899AABBCCDDEEFF");

        byte[] actual = rijndael.Encrypt(plaintext);

        assertArrayEquals(hex(expected.replace(" ", "")), actual);
    }

    // ------------------------------------------------------------------
    // Decrypt
    // ------------------------------------------------------------------

    @ParameterizedTest(name = "AES decryption with {0}-bit key")
    @CsvSource({
            // AES-128
            "000102030405060708090A0B0C0D0E0F, 69C4E0D86A7B0430D8CDB78070B4C55A",

            // AES-192
            "000102030405060708090A0B0C0D0E0F1011121314151617, DDA97CA4864CDFE06EAF70A0EC0D7191",

            // AES-256
            "000102030405060708090A0B0C0D0E0F101112131415161718191A1B1C1D1E1F, 8EA2B7CA516745BFEAFC49904B496089"
    })
    @DisplayName("Decrypt matches FIPS-197 test vectors")
    void decryptFipsTest(String key, String ciphertext) {
        Rijndael rijndael = createRijndael(key);

        byte[] expected = hex("00112233445566778899AABBCCDDEEFF");

        byte[] actual = rijndael.Decrypt(hex(ciphertext));

        assertArrayEquals(expected, actual);
    }

    // ------------------------------------------------------------------
    // Key validation
    // ------------------------------------------------------------------

    @ParameterizedTest(name = "Invalid key length: {0} bytes")
    @CsvSource({
            "0",
            "15",
            "17",
            "23",
            "25",
            "31",
            "33"
    })
    @DisplayName("Constructor rejects unsupported key sizes")
    void invalidKeySizeTest(int keyLength) {
        byte[] key = new byte[keyLength];

        assertThrows(
                IllegalArgumentException.class,
                () -> new Rijndael(key, Size.S128)
        );
    }

    @ParameterizedTest(name = "Valid key length: {0} bytes")
    @CsvSource({
            "16",
            "24",
            "32"
    })
    @DisplayName("Constructor accepts supported key sizes")
    void validKeySizeTest(int keyLength) {
        byte[] key = new byte[keyLength];

        assertDoesNotThrow(
                () -> new Rijndael(key, Size.S128)
        );
    }

    @Test
    void encryptDecryptReturnsOriginalBlock() {
        var rijndael = new Rijndael(
                hex("000102030405060708090A0B0C0D0E0F"),
                Size.S128
        );
        var originalBlock = hex("00112233445566778899AABBCCDDEEFF");

        var encryptedBlock = rijndael.Encrypt(originalBlock);
        var decryptedBlock = rijndael.Decrypt(encryptedBlock);

        assertArrayEquals(originalBlock, decryptedBlock);
    }
}
