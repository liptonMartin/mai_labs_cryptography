package common;

import models.PaddingMode;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.*;

public class PaddingTest {
    private static final int BLOCK_SIZE = 16;

    @Test
    public void testPkcs7Padding() {
        var data = new byte[]{1, 2, 3};

        var padded = Padding.addPadding(data, BLOCK_SIZE, PaddingMode.PKCS7);

        assertEquals(BLOCK_SIZE, padded.length);

        for (int i = 3; i < BLOCK_SIZE; ++i)
            assertEquals(13, padded[i] & 0xFF);

        assertArrayEquals(data, Padding.removePadding(padded, BLOCK_SIZE, PaddingMode.PKCS7));
    }

    @Test
    public void testAnsiX923Padding() {
        var data = new byte[]{1, 2, 3};

        var padded = Padding.addPadding(data, BLOCK_SIZE, PaddingMode.ANSI_X923);

        assertEquals(BLOCK_SIZE, padded.length);

        for (int i = 3; i < BLOCK_SIZE - 1; ++i)
            assertEquals(0, padded[i]);

        assertEquals(13, padded[15] & 0xFF);

        assertArrayEquals(data, Padding.removePadding(padded, BLOCK_SIZE, PaddingMode.ANSI_X923));
    }

    @Test
    public void testIso10126Padding() {
        var data = new byte[]{1, 2, 3};

        var padded = Padding.addPadding(data, BLOCK_SIZE, PaddingMode.ISO10126);

        assertEquals(BLOCK_SIZE, padded.length);
        assertEquals(13, padded[15] & 0xFF);

        assertArrayEquals(data, Padding.removePadding(padded, BLOCK_SIZE, PaddingMode.ISO10126));
    }

    @Test
    public void testZerosPadding() {
        var data = new byte[]{1, 2, 3};

        var padded = Padding.addPadding(data, BLOCK_SIZE, PaddingMode.ZEROS);

        assertEquals(BLOCK_SIZE, padded.length);
        assertArrayEquals(data, Padding.removePadding(padded, BLOCK_SIZE, PaddingMode.ZEROS));
    }

    @Test
    public void testFullBlockPkcs7Padding() {
        var data = new byte[BLOCK_SIZE];

        var padded = Padding.addPadding(data, BLOCK_SIZE, PaddingMode.PKCS7);

        assertEquals(BLOCK_SIZE * 2, padded.length);

        for (int i = BLOCK_SIZE; i < padded.length; ++i)
            assertEquals(BLOCK_SIZE, padded[i] & 0xFF);

        assertArrayEquals(data, Padding.removePadding(padded, BLOCK_SIZE, PaddingMode.PKCS7));
    }

    @Test
    public void testEmptyDataPkcs7Padding() {
        var data = new byte[0];

        var padded = Padding.addPadding(data, BLOCK_SIZE, PaddingMode.PKCS7);

        assertEquals(BLOCK_SIZE, padded.length);
        assertArrayEquals(data, Padding.removePadding(padded, BLOCK_SIZE, PaddingMode.PKCS7));
    }

    @Test
    public void testInvalidPkcs7Padding() {
        var data = new byte[BLOCK_SIZE];
        data[15] = 5;
        data[14] = 3;

        assertThrows(
                IllegalArgumentException.class,
                () -> Padding.removePadding(data, BLOCK_SIZE, PaddingMode.PKCS7)
        );
    }

    @Test
    public void testInvalidAnsiX923Padding() {
        var data = new byte[BLOCK_SIZE];
        data[14] = 1;
        data[15] = 2;

        assertThrows(
                IllegalArgumentException.class,
                () -> Padding.removePadding(data, BLOCK_SIZE, PaddingMode.ANSI_X923)
        );
    }

    @Test
    public void testInvalidPaddingLength() {
        var data = new byte[BLOCK_SIZE];
        data[15] = 0;

        assertThrows(
                IllegalArgumentException.class,
                () -> Padding.removePadding(data, BLOCK_SIZE, PaddingMode.PKCS7)
        );
    }

    @Test
    public void testNullData() {
        assertThrows(
                IllegalArgumentException.class,
                () -> Padding.addPadding(null, BLOCK_SIZE, PaddingMode.PKCS7)
        );

        assertThrows(
                IllegalArgumentException.class,
                () -> Padding.removePadding(null, BLOCK_SIZE, PaddingMode.PKCS7)
        );
    }
}
