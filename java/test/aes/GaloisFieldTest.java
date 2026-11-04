package aes;

import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.params.ParameterizedTest;
import org.junit.jupiter.params.provider.CsvSource;

import static org.junit.jupiter.api.Assertions.assertEquals;

public class GaloisFieldTest {

    @ParameterizedTest(name = "{0}+{1} = {2}")
    @CsvSource({
            "0,   0,   0",
            "1,   0,   1",
            "179,   0, 179",
            "  0, 179, 179",
            "255,   0, 255",
            "  0, 255, 255",
            "128, 128,   0",
            "127, 128, 255",
            "128, 127, 255",
            "200, 100, 172",
            "255,   2, 253",
            "179, 10, 185",
    })
    @DisplayName("Base add two numbers in GaloisField")
    void baseAddTest(int a, int b, int expected) {
        assertEquals(expected, GaloisField.add(a, b));
    }

    @ParameterizedTest(name = "{0}*{1} = {2}")
    @CsvSource({
            "179, 10, 146",
            "2,  179, 125",
            "128,  2,  27",
            "3,  179, 206",
            "255, 255,  19",
            "83,  202,   1",
            "202,  83,   1",
            "1, 255, 255",
            "283, 0, 0",
    })
    @DisplayName("Base multiply two numbers in GaloisField")
    void baseMultiplyTest(int a, int b, int expected) {
        assertEquals(expected, GaloisField.multiply(a, b));
    }

    @ParameterizedTest(name = "Is {0} irreducible? {1}")
    @CsvSource({
            "2, true",
            "3, true",

            "4, false",
            "5, false",
            "6, false",
            "7, true",

            "8, false",
            "9, false",
            "10, false",
            "11, true",
            "12, false",
            "13, true",
            "14, false",
            "15, false",

            "16, false",
            "17, false",
            "18, false",
            "19, true",
            "21, false",
            "23, false",
            "25, true",
            "27, false",
            "29, false",
            "31, true",

            "35, false",
            "37, true",
            "41, true",
            "47, true",
            "49, false",
            "55, true",
            "59, true",
            "61, true",
            "63, false",

            "67, true",
            "69, false",
            "73, true",
            "79, false",
            "87, true",
            "91, true",
            "93, false",
            "97, true",
            "103, true",
            "107, false",
            "109, true",
            "115, true",
            "117, true",
            "121, false",
            "127, false",

            "129, false",
            "131, true",
            "133, false",
            "137, true",
            "143, true",
            "145, true",
            "151, false",
            "155, false",
            "157, true",
            "161, false",
            "167, true",
            "171, true",
            "173, false",
            "179, false",
            "181, false",
            "185, true",
            "191, true",
            "193, true",
            "199, false",
            "203, true",
            "205, false",
            "211, true",
            "213, true",
            "217, false",
            "221, false",
            "223, false",
            "227, false",
            "229, true",
            "233, false",
            "239, true",
            "241, true",
            "247, true",
            "251, false",
            "253, true",
            "255, false",
    })
    @DisplayName("Base check is irreducible polynomial")
    void baseCheckIsIrreduciblePolynomial(int a, boolean expected) {
        assertEquals(expected, GaloisField.isIrreduciblePolynomial(a));
    }
}
