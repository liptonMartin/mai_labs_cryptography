package aes;

import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.params.ParameterizedTest;
import org.junit.jupiter.params.provider.CsvSource;

import java.math.BigInteger;
import java.util.ArrayList;
import java.util.List;
import java.util.Set;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertThrows;
import static org.junit.jupiter.api.Assertions.assertTrue;

public class GaloisFieldTest {

    private final GaloisField gf = new GaloisField();

    // Все 30 неприводимых полиномов степени 8, вместе со старшим битом x^8
    private static final Set<Integer> IRREDUCIBLE_8 = Set.of(
            0b1_0001_1011, 0b1_0001_1101, 0b1_0010_1011, 0b1_0010_1101, 0b1_0011_1001,
            0b1_0011_1111, 0b1_0100_1101, 0b1_0101_1111, 0b1_0110_0011, 0b1_0110_0101,
            0b1_0110_1001, 0b1_0111_0001, 0b1_0111_0111, 0b1_0111_1011, 0b1_1000_0111,
            0b1_1000_1011, 0b1_1000_1101, 0b1_1001_1111, 0b1_1010_0011, 0b1_1010_1001,
            0b1_1011_0001, 0b1_1011_1101, 0b1_1100_0011, 0b1_1100_1111, 0b1_1101_0111,
            0b1_1101_1101, 0b1_1110_0111, 0b1_1111_0011, 0b1_1111_0101, 0b1_1111_1001);

    // "1000_0000" -> (byte) 0b1000_0000, то есть -128 (в CSV значения ≥ 0b1000_0000 иначе не записать)
    private static byte b(String bits) {
        return (byte) Integer.parseInt(bits.replace("_", "").trim(), 2);
    }

    private static BigInteger poly(String bits) {
        return new BigInteger(bits.replace("_", "").trim(), 2);
    }

    private static List<BigInteger> polys(String csv) {
        List<BigInteger> result = new ArrayList<>();
        for (String s : csv.split(","))
            result.add(poly(s));
        return result;
    }

    @ParameterizedTest(name = "{0} + {1} = {2}")
    @CsvSource({
            "0000_0000, 0000_0000, 0000_0000",
            "0000_0001, 0000_0000, 0000_0001",
            "1011_0011, 0000_0000, 1011_0011",
            "0000_0000, 1011_0011, 1011_0011",
            "1111_1111, 0000_0000, 1111_1111",
            "1000_0000, 1000_0000, 0000_0000",
            "0111_1111, 1000_0000, 1111_1111",
            "1000_0000, 0111_1111, 1111_1111",
            "1100_1000, 0110_0100, 1010_1100",
            "1111_1111, 0000_0010, 1111_1101",
            "1011_0011, 0000_1010, 1011_1001",
    })
    @DisplayName("add is XOR")
    void addTest(String a, String bb, String expected) {
        assertEquals(b(expected), gf.add(b(a), b(bb)));
    }

    @ParameterizedTest(name = "{0} * {1} = {2}")
    @CsvSource({
            "1011_0011, 0000_1010, 1001_0010",
            "0000_0010, 1011_0011, 0111_1101",
            "1000_0000, 0000_0010, 0001_1011",   // x^7 * x = x^8 = x^4+x^3+x+1
            "0000_0011, 1011_0011, 1100_1110",
            "1111_1111, 1111_1111, 0001_0011",
            "0101_0011, 1100_1010, 0000_0001",   // {53} * {CA} = 1
            "1100_1010, 0101_0011, 0000_0001",
            "0000_0001, 1111_1111, 1111_1111",
            "0000_0000, 1111_1111, 0000_0000",
            "1111_1111, 0000_0000, 0000_0000",
            "0101_0111, 1000_0011, 1100_0001",   // пример из FIPS-197: {57} * {83} = {C1}
    })
    @DisplayName("multiply with default (AES) modulus")
    void multiplyDefaultModTest(String a, String bb, String expected) {
        assertEquals(b(expected), gf.multiply(b(a), b(bb)));
    }

    @ParameterizedTest(name = "{0} * {1} mod x^8+{3} = {2}")
    @CsvSource({
            "1000_0000, 0000_0010, 0001_1011, 0001_1011",
            "1000_0000, 0000_0010, 0001_1101, 0001_1101",   // x^8 = x^4+x^3+x^2+1
            "0101_0111, 1000_0011, 1100_0001, 0001_1011",
            "0000_0001, 1111_1111, 1111_1111, 0001_1101",
    })
    @DisplayName("multiply with explicit modulus")
    void multiplyExplicitModTest(String a, String bb, String expected, String mod) {
        assertEquals(b(expected), gf.multiply(b(a), b(bb), b(mod)));
    }

    @Test
    @DisplayName("multiply is commutative for every irreducible modulus")
    void multiplyCommutativeTest() {
        for (int m : IRREDUCIBLE_8) {
            byte mod = (byte) m;
            for (int x = 0; x < 256; x += 5)
                for (int y = 0; y < 256; y += 7)
                    assertEquals(gf.multiply((byte) x, (byte) y, mod), gf.multiply((byte) y, (byte) x, mod));
        }
    }

    @ParameterizedTest(name = "reducible modulus x^8+{0} throws")
    @CsvSource({
            "0000_0000",
            "0000_0001",   // x^8+1 = (x+1)^8
            "0000_0011",   // делится на x^2+x+1
            "0001_1010",   // свободный член 0, делится на x
            "1111_1111",
    })
    @DisplayName("reducible modulus throws in multiply and inverseElement")
    void reducibleModThrowsTest(String modBits) {
        byte mod = b(modBits);
        assertThrows(IllegalArgumentException.class, () -> gf.multiply((byte) 2, (byte) 3, mod));
        assertThrows(IllegalArgumentException.class, () -> gf.inverseElement((byte) 3, mod));
    }

    @ParameterizedTest(name = "inv({0}) = {1}")
    @CsvSource({
            "0000_0001, 0000_0001",
            "0101_0011, 1100_1010",   // {53}^-1 = {CA}
            "1100_1010, 0101_0011",
            "0000_0010, 1000_1101",
    })
    @DisplayName("inverseElement with default modulus")
    void inverseKnownValuesTest(String a, String expected) {
        assertEquals(b(expected), gf.inverseElement(b(a)));
    }

    @Test
    @DisplayName("a * inv(a) == 1 for every nonzero a and every irreducible modulus")
    void inverseProductIsOneTest() {
        for (int m : IRREDUCIBLE_8) {
            byte mod = (byte) m;
            for (int a = 1; a < 256; a++) {
                byte inv = gf.inverseElement((byte) a, mod);
                assertEquals(0b0000_0001, gf.multiply((byte) a, inv, mod),
                        "a=" + a + ", mod=" + m);
            }
        }
    }

    @Test
    @DisplayName("inverseElement(0) throws")
    void inverseOfZeroThrowsTest() {
        assertThrows(IllegalArgumentException.class, () -> gf.inverseElement((byte) 0));
    }

    // ---------- irreducibility of degree 8 ----------

    @Test
    @DisplayName("isIrreduciblePolynomial8Degree matches the reference list for all 256 values")
    void isIrreducible8DegreeAllTest() {
        for (int low = 0; low < 256; low++) {
            boolean expected = IRREDUCIBLE_8.contains(0b1_0000_0000 | low);
            assertEquals(expected, gf.isIrreduciblePolynomial8Degree((byte) low),
                    "x^8 + 0b" + Integer.toBinaryString(low));
        }
    }

    @ParameterizedTest(name = "x^8+{0} irreducible = {1}")
    @CsvSource({
            "0001_1011, true",
            "0001_1101, true",
            "1111_1001, true",
            "0000_0011, false",   // x^8+x+1 делится на x^2+x+1
            "0000_0010, false",
            "0001_1010, false",
            "0000_0001, false",
            "1000_0000, false",
            "1111_1111, false",
    })
    @DisplayName("isIrreduciblePolynomial8Degree on selected values")
    void isIrreducible8DegreeSelectedTest(String modBits, boolean expected) {
        assertEquals(expected, gf.isIrreduciblePolynomial8Degree(b(modBits)));
    }

    @Test
    @DisplayName("generateAllIrreduciblePolynomials8Degree returns exactly 30, ascending")
    void generateAllTest() {
        List<Byte> result = gf.generateAllIrreduciblePolynomials8Degree();

        List<Byte> expected = new ArrayList<>();
        IRREDUCIBLE_8.stream().sorted().forEach(p -> expected.add((byte) (int) p));   // старший бит x^8 отбрасывается

        assertEquals(30, result.size());
        assertEquals(expected, result);
    }

    // ---------- decomposition ----------

    @ParameterizedTest(name = "{0} in GF(2^{1}) = {2}")
    @CsvSource({
            "11, 8, '11'",
            "111, 8, '111'",
            "1011, 8, '1011'",
            "1101, 8, '1101'",

            "1001, 8, '11, 111'",
            "111_1111, 8, '1011, 1101'",
            "1_1011_1011, 8, '1_0011, 1_1001'",

            "100, 8, '10, 10'",
            "101, 8, '11, 11'",
            "1111, 8, '11, 11, 11'",
            "1_0101, 8, '111, 111'",
            "1100_1111, 8, '11, 1011, 1011'",

            "1_0010, 8, '10, 11, 111'",
            "1_0111_1101, 8, '111, 1011, 1101'",

            "11, 2, '11'",
            "111, 3, '111'",
            "1011, 4, '1011'",

            "101, 2, '11, 11'",
            "1001, 3, '11, 111'",
            "1_0101, 3, '111, 111'",
            "100_0101, 4, '1011, 1011'",

            "110, 2, '10, 11'",
            "1010, 2, '10, 11, 11'",
            "1_0010, 3, '10, 11, 111'",
    })
    @DisplayName("decompose into irreducible factors")
    void decomposeTest(String a, int n, String expected) {
        assertEquals(polys(expected), gf.decomposePolynomialToIrreducible(poly(a), n));
    }

    @ParameterizedTest(name = "{0} is not decomposable in GF(2^{1})")
    @CsvSource({
            "1_0001_1011, 8",   // сам неприводим и имеет степень 8
            "101, 1",
            "1001, 1",
            "1001, 2",          // 111 имеет степень 2
            "1_0101, 2",
            "100_0101, 3",      // 1011 имеет степень 3
            "11_1011, 2",
            "10, 1",
            "1010, 1",
    })
    @DisplayName("decompose throws when a factor is not an element of GF(2^n)")
    void decomposeNotInFieldThrowsTest(String a, int n) {
        assertThrows(IllegalArgumentException.class,
                () -> gf.decomposePolynomialToIrreducible(poly(a), n));
    }

    @Test
    @DisplayName("decompose(0) throws")
    void decomposeZeroThrowsTest() {
        assertThrows(IllegalArgumentException.class,
                () -> gf.decomposePolynomialToIrreducible(BigInteger.ZERO, 8));
    }

    @Test
    @DisplayName("decompose(1) is an empty product")
    void decomposeOneTest() {
        assertTrue(gf.decomposePolynomialToIrreducible(BigInteger.ONE, 8).isEmpty());
    }
}