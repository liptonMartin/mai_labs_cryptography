package common;

import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.params.ParameterizedTest;
import org.junit.jupiter.params.provider.CsvSource;

import java.math.BigInteger;

import static org.junit.jupiter.api.Assertions.assertEquals;

class HelperTest {

    @ParameterizedTest(name = "{0}^{1} mod {2} = {3}")
    @CsvSource({
            "2,   10,  1000,  24",
            "5,   3,   1000,  125",
            "1,   100, 1000,  1",
            "0,   5,   1000,  0",
            "0,   0,   1000,  1",
            "2,   0,   1000,  1",
            "7,   0,   1,     0",
            "5,   100, 1,     0"
    })
    @DisplayName("Базовые случаи a^n mod m")
    void should_compute_power_mod(int a, int n, int mod, int expected) {
        assertEquals(expected, Helper.binaryPow(a, n, mod));
    }

    @Test
    @DisplayName("a = mod → 0 при n > 0")
    void should_return_zero_when_base_equals_modulus() {
        assertEquals(0, Helper.binaryPow(1000, 5, 1000));
    }

    @Test
    @DisplayName("Не должно быть переполнения при возведении 46341 в квадрат")
    void should_not_overflow_when_squaring_large_base() {
        assertEquals(147_488_267, Helper.binaryPow(46341, 2, 1_000_000_007));
    }

    @Test
    @DisplayName("Не должно быть переполнения при большом модуле")
    void should_not_overflow_with_large_modulus() {
        int mod = 1_000_000_007;
        int expected = BigInteger.valueOf(46341)
                .modPow(BigInteger.valueOf(10), BigInteger.valueOf(mod))
                .intValue();
        assertEquals(expected, Helper.binaryPow(46341, 10, mod));
    }

    @Test
    @DisplayName("2^30 mod 1_000_000_007")
    void should_compute_2_pow_30() {
        assertEquals(73_741_817, Helper.binaryPow(2, 30, 1_000_000_007));
    }

    @Test
    @DisplayName("Совпадает с BigInteger.modPow на наборе значений")
    void should_match_big_integer_mod_pow() {
        int[][] cases = {
                {2,     1_000_000_006, 1_000_000_007},
                {3,     100_000_000,   1_000_000_007},
                {12345, 999_999,       1_000_000_007},
                {7,     123_456_789,   998_244_353},
                {123,   1_000_000,     1_000_000_007}
        };
        for (int[] c : cases) {
            int expected = BigInteger.valueOf(c[0])
                    .modPow(BigInteger.valueOf(c[1]), BigInteger.valueOf(c[2]))
                    .intValue();
            assertEquals(
                    expected,
                    Helper.binaryPow(c[0], c[1], c[2]),
                    () -> String.format("%d^%d mod %d", c[0], c[1], c[2])
            );
        }
    }

    @Test
    @DisplayName("Малая теорема Ферма: a^(p−1) ≡ 1 (mod p) для простого p")
    void should_satisfy_fermat_little_theorem() {
        int p = 1_000_000_007;
        int[] bases = {2, 3, 5, 7, 11, 1_000_000_006};
        for (int a : bases) {
            assertEquals(1, Helper.binaryPow(a, p - 1, p),
                    () -> "Fermat failed for a = " + a);
        }
    }

    @Test
    @DisplayName("a = Integer.MAX_VALUE, n = 1, mod = a → 0")
    void should_handle_max_int_base() {
        int max = Integer.MAX_VALUE;
        assertEquals(0, Helper.binaryPow(max, 1, max));
    }

    @Test
    @DisplayName("a = Integer.MAX_VALUE, n = 2, mod = 1_000_000_007")
    void should_handle_max_int_base_squared() {
        long expected = ((long) Integer.MAX_VALUE * Integer.MAX_VALUE) % 1_000_000_007L;
        assertEquals((int) expected, Helper.binaryPow(Integer.MAX_VALUE, 2, 1_000_000_007));
    }
}