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

public class RijndaelTest {

    private Rijndael rijndael;
    private byte[][] sBox;
    private byte[][] inverseSBox;

    @BeforeEach
    void setUp() throws Exception {
        rijndael = new Rijndael(Size.S128, Size.S128);
        sBox = invoke(rijndael, "generateSBox");
        inverseSBox = invoke(rijndael, "generateInverseSBox");
    }

    // ------------------------------------------------------------------
    // helpers
    // ------------------------------------------------------------------

    // "0101_0011" -> (byte) 0x53 (в CSV значения ≥ 0b1000_0000 иначе не записать)
    private static byte b(String bits) {
        return (byte) Integer.parseInt(bits.replace("_", "").trim(), 2);
    }

    private static byte[][] invoke(Rijndael rijndael, String methodName) throws Exception {
        Method method = Rijndael.class.getDeclaredMethod(methodName);
        method.setAccessible(true);
        return (byte[][]) method.invoke(rijndael);
    }

    private static byte[][] invokeState(Rijndael rijndael, String methodName, byte[][] state) throws Exception {
        Method method = Rijndael.class.getDeclaredMethod(methodName, byte[][].class);
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
}