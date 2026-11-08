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

    private byte[][] sBox;
    private byte[][] inverseSBox;

    @BeforeEach
    void setUp() throws Exception {
        Rijndael rijndael = new Rijndael(Size.S128, Size.S128);
        sBox = invoke(rijndael, "generateSBox");
        inverseSBox = invoke(rijndael, "generateInverseSBox");
    }

    // "0101_0011" -> (byte) 0x53 (в CSV значения ≥ 0b1000_0000 иначе не записать)
    private static byte b(String bits) {
        return (byte) Integer.parseInt(bits.replace("_", "").trim(), 2);
    }

    private static byte[][] invoke(Rijndael rijndael, String methodName) throws Exception {
        Method method = Rijndael.class.getDeclaredMethod(methodName);
        method.setAccessible(true);
        return (byte[][]) method.invoke(rijndael);
    }

    // старший ниббл -> строка, младший -> столбец
    private static byte lookup(byte[][] table, byte x) {
        int v = Byte.toUnsignedInt(x);
        return table[v >>> 4][v & 0xF];
    }

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
}