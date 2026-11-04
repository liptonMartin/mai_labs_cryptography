package aes;

import common.Helper;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

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

    private static int remainder(long a, int mod) {
        for (int i = 15; i >= 8; --i) {
            if (((a >> i) & 1) == 0)
                continue;

            a ^= (long) mod << (i - 8);
        }

        return (int) a;
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
        var irreduciblePolynomials = generateIrreduciblePolynomials();

        var maxPolynomialDegree = 0;
        for (int i = 8; i >= 0; --i)
            if (((a >> i) & 1) == 1) {
                maxPolynomialDegree = i;
                break;
            }

        for (int i = 0; i < 4; ++i) {
            for (int j = i; i + j <= maxPolynomialDegree; ++j) {
                var listI = irreduciblePolynomials.get(i);
                var listJ = irreduciblePolynomials.get(j);

                for (var numberI : listI) {
                    for (var numberJ : listJ) {
                        var multiplyResult = multiply(numberI, numberJ);
                        if (multiplyResult == a && (numberI != 1 || numberJ == 1))
                            return false;
                    }
                }
            }
        }
        return true;
    }

    private static Map<Integer, List<Integer>> generateIrreduciblePolynomials() {
        Map<Integer, List<Integer>> result = new HashMap<>();

        for (int i = 0; i < 8; ++i) {
            var countNumbers = 1 << (i + 1);
            var numbers = new ArrayList<Integer>(countNumbers);
            for (int j = i + 1; j < countNumbers; ++j) {
                numbers.add(j);
            }
            result.put(i, numbers);
        }

        return result;
    }
}

