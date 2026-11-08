package aes;

import models.Size;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.params.ParameterizedTest;
import org.junit.jupiter.params.provider.CsvSource;

import java.lang.reflect.Method;
import java.util.HashSet;
import java.util.Set;

import static org.junit.jupiter.api.Assertions.*;

public class RijndaelOperationsTest {

    private RijndaelOperations rijndael;
    private byte[][] sBox;
    private byte[][] inverseSBox;

    @BeforeEach
    void setUp() throws Exception {
        rijndael = new RijndaelOperations(createGaloisField(), Size.S128);
        sBox = invoke(rijndael, "generateSBox");
        inverseSBox = invoke(rijndael, "generateInverseSBox");
    }

    // ------------------------------------------------------------------
    // helpers
    // ------------------------------------------------------------------

    private static GaloisField createGaloisField() {
        return new GaloisField();
    }

    // "0101_0011" -> (byte) 0x53 (в CSV значения ≥ 0b1000_0000 иначе не записать)
    private static byte b(String bits) {
        return (byte) Integer.parseInt(bits.replace("_", "").trim(), 2);
    }

    private static byte[][] invoke(RijndaelOperations rijndael, String methodName) throws Exception {
        Method method = RijndaelOperations.class.getDeclaredMethod(methodName);
        method.setAccessible(true);
        return (byte[][]) method.invoke(rijndael);
    }

    private static byte[][] invokeState(RijndaelOperations rijndael, String methodName, byte[][] state) throws Exception {
        Method method = RijndaelOperations.class.getDeclaredMethod(methodName, byte[][].class);
        method.setAccessible(true);
        return (byte[][]) method.invoke(rijndael, (Object) state);
    }

    // старший ниббл -> строка, младший -> столбец
    private static byte lookup(byte[][] table, byte x) {
        int v = Byte.toUnsignedInt(x);
        return table[v >>> 4][v & 0xF];
    }

    // int[] -> byte[], чтобы писать значения в hex: row(0x19, 0x3D, ...)
    private static byte[] row(int... values) {
        byte[] result = new byte[values.length];
        for (int i = 0; i < values.length; i++)
            result[i] = (byte) values[i];
        return result;
    }

    private static byte[][] copy(byte[][] state) {
        byte[][] result = new byte[state.length][];
        for (int i = 0; i < state.length; i++)
            result[i] = state[i].clone();
        return result;
    }

    // state[r][c] = r * 16 + c, то есть все 256 значений ровно по одному разу
    private static byte[][] allBytesState() {
        byte[][] state = new byte[16][16];
        for (int r = 0; r < 16; r++)
            for (int c = 0; c < 16; c++)
                state[r][c] = (byte) (r * 16 + c);
        return state;
    }

    // FIPS-197, Appendix B, раунд 1: "Start of Round" -> "After SubBytes"
    private static byte[][] fipsStateBefore() {
        return new byte[][]{
                row(0x19, 0x3D, 0xE3, 0xBE),
                row(0xA0, 0xF4, 0xE2, 0x2B),
                row(0x9A, 0xC6, 0x8D, 0x2A),
                row(0xE9, 0xF8, 0x48, 0x08),
        };
    }

    private static byte[][] fipsStateAfter() {
        return new byte[][]{
                row(0xD4, 0x27, 0x11, 0xAE),
                row(0xE0, 0xBF, 0x98, 0xF1),
                row(0xB8, 0xB4, 0x5D, 0xE5),
                row(0x1E, 0x41, 0x52, 0x30),
        };
    }

    private static byte[][] invokeAddRoundKey(
            RijndaelOperations rijndael,
            byte[][] state,
            byte[][] roundKey
    ) throws Exception {
        Method method = RijndaelOperations.class.getDeclaredMethod(
                "addRoundKey",
                byte[][].class,
                byte[][].class
        );
        method.setAccessible(true);
        return (byte[][]) method.invoke(rijndael, state, roundKey);
    }

    // ------------------------------------------------------------------
    // S-box / inverse S-box
    // ------------------------------------------------------------------

    @Test
    @DisplayName("S-box has 16x16 shape")
    void sBoxShapeTest() {
        assertEquals(16, sBox.length);
        for (byte[] row : sBox)
            assertEquals(16, row.length);
    }

    @Test
    @DisplayName("inverse S-box has 16x16 shape")
    void inverseSBoxShapeTest() {
        assertEquals(16, inverseSBox.length);
        for (byte[] row : inverseSBox)
            assertEquals(16, row.length);
    }

    @ParameterizedTest(name = "S({0}) = {1}")
    @CsvSource({
            "0000_0000, 0110_0011",   // S(00) = 63
            "0000_0001, 0111_1100",   // S(01) = 7C
            "0001_0000, 1100_1010",   // S(10) = CA
            "0010_0000, 1011_0111",   // S(20) = B7
            "0101_0010, 0000_0000",   // S(52) = 00
            "0101_0011, 1110_1101",   // S(53) = ED
            "1001_1010, 1011_1000",   // S(9A) = B8
            "1100_1001, 1101_1101",   // S(C9) = DD
            "1111_0000, 1000_1100",   // S(F0) = 8C
            "1111_1111, 0001_0110",   // S(FF) = 16
    })
    @DisplayName("S-box matches FIPS-197 values")
    void sBoxValuesTest(String input, String expected) {
        assertEquals(b(expected), lookup(sBox, b(input)));
    }

    @ParameterizedTest(name = "InvS({0}) = {1}")
    @CsvSource({
            "0000_0000, 0101_0010",   // InvS(00) = 52
            "0000_0001, 0000_1001",   // InvS(01) = 09
            "0110_0011, 0000_0000",   // InvS(63) = 00
            "0111_1100, 0000_0001",   // InvS(7C) = 01
            "1110_1101, 0101_0011",   // InvS(ED) = 53
            "1011_1000, 1001_1010",   // InvS(B8) = 9A
            "1111_0000, 0001_0111",   // InvS(F0) = 17
            "0001_0110, 1111_1111",   // InvS(16) = FF
            "1111_1111, 0111_1101",   // InvS(FF) = 7D
    })
    @DisplayName("inverse S-box matches FIPS-197 values")
    void inverseSBoxValuesTest(String input, String expected) {
        assertEquals(b(expected), lookup(inverseSBox, b(input)));
    }

    @Test
    @DisplayName("S-box is a permutation of all 256 byte values")
    void sBoxIsPermutationTest() {
        Set<Byte> seen = new HashSet<>();
        for (byte[] row : sBox)
            for (byte value : row)
                seen.add(value);
        assertEquals(256, seen.size());
    }

    @Test
    @DisplayName("inverse S-box is a permutation of all 256 byte values")
    void inverseSBoxIsPermutationTest() {
        Set<Byte> seen = new HashSet<>();
        for (byte[] row : inverseSBox)
            for (byte value : row)
                seen.add(value);
        assertEquals(256, seen.size());
    }

    @Test
    @DisplayName("InvS(S(x)) = x for every byte")
    void inverseOfSBoxTest() {
        for (int x = 0; x < 256; x++) {
            byte value = (byte) x;
            assertEquals(value, lookup(inverseSBox, lookup(sBox, value)), "x = " + x);
        }
    }

    @Test
    @DisplayName("S(InvS(x)) = x for every byte")
    void sBoxOfInverseTest() {
        for (int x = 0; x < 256; x++) {
            byte value = (byte) x;
            assertEquals(value, lookup(sBox, lookup(inverseSBox, value)), "x = " + x);
        }
    }

    @Test
    @DisplayName("S-box has no fixed points and no opposite fixed points")
    void sBoxNoFixedPointsTest() {
        for (int x = 0; x < 256; x++) {
            byte value = (byte) x;
            byte substituted = lookup(sBox, value);
            assertNotEquals(value, substituted, "fixed point at x = " + x);
            assertNotEquals((byte) ~value, substituted, "opposite fixed point at x = " + x);
        }
    }

    // ------------------------------------------------------------------
    // subBytes / inverseSubBytes
    // ------------------------------------------------------------------

    @Test
    @DisplayName("subBytes matches FIPS-197 Appendix B example")
    void subBytesFipsExampleTest() throws Exception {
        byte[][] actual = invokeState(rijndael, "subBytes", fipsStateBefore());
        assertArrayEquals(fipsStateAfter(), actual);
    }

    @Test
    @DisplayName("inverseSubBytes matches FIPS-197 Appendix B example")
    void inverseSubBytesFipsExampleTest() throws Exception {
        byte[][] actual = invokeState(rijndael, "inverseSubBytes", fipsStateAfter());
        assertArrayEquals(fipsStateBefore(), actual);
    }

    @Test
    @DisplayName("subBytes applied to all 256 bytes equals the S-box table")
    void subBytesCoversWholeSBoxTest() throws Exception {
        byte[][] actual = invokeState(rijndael, "subBytes", allBytesState());
        assertArrayEquals(sBox, actual);
    }

    @Test
    @DisplayName("inverseSubBytes applied to all 256 bytes equals the inverse S-box table")
    void inverseSubBytesCoversWholeInverseSBoxTest() throws Exception {
        byte[][] actual = invokeState(rijndael, "inverseSubBytes", allBytesState());
        assertArrayEquals(inverseSBox, actual);
    }

    @Test
    @DisplayName("inverseSubBytes(subBytes(state)) = state")
    void inverseSubBytesUndoesSubBytesTest() throws Exception {
        byte[][] original = allBytesState();
        byte[][] substituted = invokeState(rijndael, "subBytes", copy(original));
        byte[][] restored = invokeState(rijndael, "inverseSubBytes", substituted);
        assertArrayEquals(original, restored);
    }

    @Test
    @DisplayName("subBytes(inverseSubBytes(state)) = state")
    void subBytesUndoesInverseSubBytesTest() throws Exception {
        byte[][] original = allBytesState();
        byte[][] substituted = invokeState(rijndael, "inverseSubBytes", copy(original));
        byte[][] restored = invokeState(rijndael, "subBytes", substituted);
        assertArrayEquals(original, restored);
    }

    @ParameterizedTest(name = "state 4x{0}")
    @CsvSource({"4", "6", "8"})   // Nb = 4, 6, 8 для блоков 128/192/256 бит
    @DisplayName("subBytes works for every state width and matches per-byte S-box lookup")
    void subBytesDifferentWidthsTest(int columns) throws Exception {
        byte[][] input = new byte[4][columns];
        byte[][] expected = new byte[4][columns];
        for (int r = 0; r < 4; r++) {
            for (int c = 0; c < columns; c++) {
                byte value = (byte) (r * 37 + c * 11 + 5);
                input[r][c] = value;
                expected[r][c] = lookup(sBox, value);
            }
        }

        assertArrayEquals(expected, invokeState(rijndael, "subBytes", input));
    }

    @Test
    @DisplayName("subBytes of an all-zero state gives S(00) everywhere")
    void subBytesZeroStateTest() throws Exception {
        byte[][] actual = invokeState(rijndael, "subBytes", new byte[4][4]);
        for (byte[] r : actual)
            for (byte v : r)
                assertEquals((byte) 0x63, v);
    }

    @Test
    @DisplayName("inverseSubBytes of an all-zero state gives InvS(00) everywhere")
    void inverseSubBytesZeroStateTest() throws Exception {
        byte[][] actual = invokeState(rijndael, "inverseSubBytes", new byte[4][4]);
        for (byte[] r : actual)
            for (byte v : r)
                assertEquals((byte) 0x52, v);
    }

    @ParameterizedTest(name = "ShiftRows for block size {0} columns")
    @CsvSource({
            "4, 1, 2, 3",
            "6, 1, 2, 3",
            "8, 1, 3, 4"
    })
    @DisplayName("shiftRows performs correct cyclic left shifts for every block size")
    void shiftRowsDifferentBlockSizesTest(
            int columns,
            int shiftRow1,
            int shiftRow2,
            int shiftRow3
    ) throws Exception {
        Size blockSize = switch (columns) {
            case 4 -> Size.S128;
            case 6 -> Size.S192;
            case 8 -> Size.S256;
            default -> throw new IllegalArgumentException("Unsupported block size");
        };

        RijndaelOperations algorithm = new RijndaelOperations(createGaloisField(), blockSize);

        byte[][] input = new byte[4][columns];
        byte[][] expected = new byte[4][columns];

        int[] shifts = {0, shiftRow1, shiftRow2, shiftRow3};

        // Сначала полностью заполняем входную матрицу
        for (int r = 0; r < 4; r++) {
            for (int c = 0; c < columns; c++) {
                input[r][c] = (byte) (r * 32 + c);
            }
        }

        // Затем независимо формируем ожидаемый результат
        for (int r = 0; r < 4; r++) {
            for (int c = 0; c < columns; c++) {
                expected[r][c] = input[r][(c + shifts[r]) % columns];
            }
        }

        byte[][] actual = invokeState(algorithm, "shiftRows", copy(input));

        assertArrayEquals(expected, actual);
    }

    // ------------------------------------------------------------------
// addRoundKey
// ------------------------------------------------------------------

    @Test
    @DisplayName("addRoundKey XORs state with round key")
    void addRoundKeyTest() throws Exception {
        byte[][] state = {
                row(0x19, 0xA0, 0x9A, 0xE9),
                row(0x3D, 0xF4, 0xC6, 0xF8),
                row(0xE3, 0xE2, 0x8D, 0x48),
                row(0xBE, 0x2B, 0x2A, 0x08)
        };

        byte[][] roundKey = {
                row(0xA0, 0xFA, 0xFE, 0x17),
                row(0x88, 0x54, 0x2C, 0xB1),
                row(0x23, 0xA3, 0x39, 0x39),
                row(0x2A, 0x6C, 0x76, 0x05)
        };

        byte[][] expected = {
                row(0xB9, 0x5A, 0x64, 0xFE),
                row(0xB5, 0xA0, 0xEA, 0x49),
                row(0xC0, 0x41, 0xB4, 0x71),
                row(0x94, 0x47, 0x5C, 0x0D)
        };

        byte[][] actual = invokeAddRoundKey(rijndael, copy(state), roundKey);

        assertArrayEquals(expected, actual);
    }

    @Test
    @DisplayName("addRoundKey with zero round key does not change state")
    void addRoundKeyZeroKeyTest() throws Exception {
        byte[][] state = {
                row(0x19, 0x3D, 0xE3, 0xBE),
                row(0xA0, 0xF4, 0xE2, 0x2B),
                row(0x9A, 0xC6, 0x8D, 0x2A),
                row(0xE9, 0xF8, 0x48, 0x08)
        };

        byte[][] roundKey = new byte[4][4];

        byte[][] actual = invokeAddRoundKey(rijndael, copy(state), roundKey);

        assertArrayEquals(state, actual);
    }

    @Test
    @DisplayName("addRoundKey with identical state and round key produces zero state")
    void addRoundKeyIdenticalMatricesTest() throws Exception {
        byte[][] state = {
                row(0x19, 0x3D, 0xE3, 0xBE),
                row(0xA0, 0xF4, 0xE2, 0x2B),
                row(0x9A, 0xC6, 0x8D, 0x2A),
                row(0xE9, 0xF8, 0x48, 0x08)
        };

        byte[][] expected = new byte[4][4];

        byte[][] actual = invokeAddRoundKey(rijndael, copy(state), copy(state));

        assertArrayEquals(expected, actual);
    }

    @Test
    @DisplayName("addRoundKey applied twice with the same key restores original state")
    void addRoundKeyUndoesItselfTest() throws Exception {
        byte[][] original = {
                row(0x19, 0x3D, 0xE3, 0xBE),
                row(0xA0, 0xF4, 0xE2, 0x2B),
                row(0x9A, 0xC6, 0x8D, 0x2A),
                row(0xE9, 0xF8, 0x48, 0x08)
        };

        byte[][] roundKey = {
                row(0xA0, 0xFA, 0xFE, 0x17),
                row(0x88, 0x54, 0x2C, 0xB1),
                row(0x23, 0xA3, 0x39, 0x39),
                row(0x2A, 0x6C, 0x76, 0x05)
        };

        byte[][] state = invokeAddRoundKey(rijndael, copy(original), roundKey);
        byte[][] actual = invokeAddRoundKey(rijndael, state, roundKey);

        assertArrayEquals(original, actual);
    }

    @ParameterizedTest(name = "state 4x{0}")
    @CsvSource({"4", "6", "8"})
    @DisplayName("addRoundKey works for every supported block size")
    void addRoundKeyDifferentWidthsTest(int columns) throws Exception {
        byte[][] state = new byte[4][columns];
        byte[][] roundKey = new byte[4][columns];
        byte[][] expected = new byte[4][columns];

        for (int r = 0; r < 4; r++) {
            for (int c = 0; c < columns; c++) {
                state[r][c] = (byte) (r * 37 + c * 11 + 5);
                roundKey[r][c] = (byte) (r * 17 + c * 23 + 9);
                expected[r][c] = (byte) (state[r][c] ^ roundKey[r][c]);
            }
        }

        byte[][] actual = invokeAddRoundKey(rijndael, copy(state), roundKey);

        assertArrayEquals(expected, actual);
    }

    @Test
    @DisplayName("addRoundKey modifies the original state")
    void addRoundKeyModifiesOriginalStateTest() throws Exception {
        byte[][] state = {
                row(0x01, 0x02, 0x03, 0x04),
                row(0x05, 0x06, 0x07, 0x08),
                row(0x09, 0x0A, 0x0B, 0x0C),
                row(0x0D, 0x0E, 0x0F, 0x10)
        };

        byte[][] roundKey = {
                row(0x10, 0x20, 0x30, 0x40),
                row(0x50, 0x60, 0x70, 0x80),
                row(0x90, 0xA0, 0xB0, 0xC0),
                row(0xD0, 0xE0, 0xF0, 0xFF)
        };

        byte[][] actual = invokeAddRoundKey(rijndael, state, roundKey);

        assertSame(state, actual);
    }


    @Test
    @DisplayName("mixColumns matches FIPS-197 example")
    void mixColumnsFipsExampleTest() throws Exception {
        byte[][] state = {
                row(0x87, 0xF2, 0x4D, 0x97),
                row(0x6E, 0x4C, 0x90, 0xEC),
                row(0x46, 0xE7, 0x4A, 0xC3),
                row(0xA6, 0x8C, 0xD8, 0x95)
        };

        byte[][] expected = {
                row(0x47, 0x40, 0xA3, 0x4C),
                row(0x37, 0xD4, 0x70, 0x9F),
                row(0x94, 0xE4, 0x3A, 0x42),
                row(0xED, 0xA5, 0xA6, 0xBC)
        };

        byte[][] actual = invokeState(rijndael, "mixColumns", copy(state));

        assertArrayEquals(expected, actual);
    }

    @Test
    @DisplayName("mixColumns of zero state gives zero state")
    void mixColumnsZeroStateTest() throws Exception {
        byte[][] state = new byte[4][4];

        byte[][] actual = invokeState(rijndael, "mixColumns", state);

        assertArrayEquals(new byte[4][4], actual);
    }

    @Test
    @DisplayName("mixColumns preserves columns containing identical bytes")
    void mixColumnsIdenticalColumnBytesTest() throws Exception {
        byte[][] state = {
                row(0x01, 0x11, 0xA5, 0xFF),
                row(0x01, 0x11, 0xA5, 0xFF),
                row(0x01, 0x11, 0xA5, 0xFF),
                row(0x01, 0x11, 0xA5, 0xFF)
        };

        byte[][] actual = invokeState(rijndael, "mixColumns", copy(state));

        assertArrayEquals(state, actual);
    }

    @ParameterizedTest(name = "state 4x{0}")
    @CsvSource({"4", "6", "8"})
    @DisplayName("mixColumns works for every supported block size")
    void mixColumnsDifferentWidthsTest(int columns) throws Exception {
        byte[][] state = new byte[4][columns];

        for (int r = 0; r < 4; r++) {
            for (int c = 0; c < columns; c++) {
                state[r][c] = (byte) (r * 37 + c * 11 + 5);
            }
        }

        byte[][] actual = invokeState(rijndael, "mixColumns", copy(state));

        assertEquals(4, actual.length);

        for (byte[] row : actual) {
            assertEquals(columns, row.length);
        }
    }


}