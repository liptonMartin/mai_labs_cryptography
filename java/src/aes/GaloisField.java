package aes;

import common.Helper;

import java.util.ArrayList;

public class GaloisField {
    static int defaultMod = 283;
    static int cardinality = 256;

    public static int add(int a, int b) {
        return a ^ b;
    }

    public static int multiply(int a, int b) {
        long result = 0;
        for (int i = 0; i <= 7; ++i) {
            if ((b & 1) != 0)
                result ^= (long) a << i;
            b >>= 1;
        }

        return remainder(result, defaultMod);
    }

    public static int inverseElement(int a) {
        return inverseElementMod(a, defaultMod);
    }

    public static int inverseElement(int a, int mod) {
        return inverseElementMod(a, mod);
    }

    private static int inverseElementMod(int a, int mod) {
        return Helper.binaryPow(a, cardinality - 2, mod);
    }

    public static boolean isIrreduciblePolynomial(int a) {
        if (a == 2 || a == 3)
            return true;

        if (remainder(a, 2) == 0)
            return false;

        var bitLength = getBitLength(a);
        var halfBitLength = bitLength / 2 + bitLength % 2;
        for (int divisor = 3; divisor <= (1 << halfBitLength); divisor += 2) {
            if (remainder(a, divisor) == 0)
                return false;
        }
        return true;
    }

    private static int remainder(long dividend, int divisor) {
        var bitLengthDividend = getBitLength(dividend);
        var bitLengthDivisor = getBitLength(divisor);
        for (int i = bitLengthDividend - 1; i >= bitLengthDivisor - 1; --i) {
            if (((dividend >> i) & 1) == 0)
                continue;

            dividend ^= (long) divisor << (i - bitLengthDivisor + 1);
        }

        return (int) dividend;
    }

    public static ArrayList<Integer> generateAllIrreduciblePolynomials8Degree() {
        var result = new ArrayList<Integer>();
        for (int i = (1 << 8); i < (1 << 9); ++i)
            if (isIrreduciblePolynomial(i))
                result.add(i);

        return result;
    }

    private static int getBitLength(long a) {
        var bitLength = 0;
        while (a > 0) {
            ++bitLength;
            a >>= 1;
        }
        return bitLength;
    }
}

