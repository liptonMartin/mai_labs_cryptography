package aes;

import java.math.BigInteger;
import java.util.ArrayList;
import java.util.List;

public final class GaloisField {
    private final byte defaultMod = 0b0001_1011;

    public byte add(byte a, byte b) {
        return (byte) (a ^ b);
    }

    public byte multiply(byte a, byte b) {
        return multiplyMod(a, b, defaultMod);
    }

    public byte multiply(byte a, byte b, byte mod) {
        return multiplyMod(a, b, mod);
    }

    private byte multiplyMod(byte a, byte b, byte mod) {
        if (!isIrreduciblePolynomial8Degree(mod))
            throw new IllegalArgumentException(
                    String.format(
                            "The mod %s (degree 8, unsigned = %s) is not irreducible in GF(2^8)",
                            mod,
                            Byte.toUnsignedInt(mod)
                    )
            );

        byte result = 0;
        for (int i = 0; i <= 7; ++i) {
            if ((b & 1) != 0) {
                result ^= reduce8Degree((a & 0xFF) << i, mod);
            }
            b >>= 1;
        }

        return result;
    }

    public byte inverseElement(byte a) {
        return inverseElementMod(a, defaultMod);
    }

    public byte inverseElement(byte a, byte mod) {
        return inverseElementMod(a, mod);
    }

    private byte inverseElementMod(byte a, byte mod) {
        if (!isIrreduciblePolynomial8Degree(mod))
            throw new IllegalArgumentException(
                    String.format(
                            "The mod %s (degree 8, unsigned = %s) is not irreducible in GF(2^8)",
                            mod,
                            Byte.toUnsignedInt(mod)
                    )
            );

        if (a == 0)
            throw new IllegalArgumentException("The number 0 is not irreducibly in GF(2^8)");
        return binaryPow(a, (byte) 254, mod);
    }

    private byte binaryPow(byte a, byte n, byte mod) {
        if (n == 0)
            return reduce8Degree(1, mod);

        byte result = 1;
        byte base = a;
        while (n != 0) {
            if ((n & 1) == 1)
                result = multiply(result, base, mod);

            base = multiply(base, base, mod);
            n = (byte) ((n & 0xFF) >> 1);
        }

        return result;
    }

    public boolean isIrreduciblePolynomial8Degree(byte a) {
        if (a == 2)
            return false;

        if ((a & 1) == 0) // a % 2 == 0
            return false;

        var bitLength = getBitLength((a & 0xFF) | 0b1_0000_0000);
        var halfBitLength = bitLength / 2 + bitLength % 2;
        for (var divisor = 3; divisor <= (1 << halfBitLength); divisor += 2) {
            var polynomial = (a & 0xFF) | 0b1_0000_0000;
            if (remainder(polynomial, (byte) divisor) == 0)
                return false;
        }
        return true;
    }

    public List<Byte> generateAllIrreduciblePolynomials8Degree() {
        var result = new ArrayList<Byte>();
        for (var i = 0; i < 256; ++i) {
            var polynomial = (byte) i;
            if (isIrreduciblePolynomial8Degree(polynomial))
                result.add(polynomial);
        }

        return List.copyOf(result);
    }

    public List<BigInteger> decomposePolynomialToIrreducible(BigInteger a, int n) {
        var initialA = a;
        if (a.compareTo(BigInteger.ZERO) == 0)
            throw new IllegalArgumentException("Number 0 is undecomposed!");

        var result = new ArrayList<BigInteger>();

        while (remainder(a, BigInteger.TWO).compareTo(BigInteger.ZERO) == 0) { // remainder == 0
            a = divide(a, BigInteger.TWO);
            result.add(BigInteger.TWO);
        }

        BigInteger divisor = new BigInteger("3");
        while (2 * (divisor.bitLength() - 1) <= a.bitLength() - 1) { // 2 * degree(divisor) <= a * degree(a)
            while (remainder(a, divisor).compareTo(BigInteger.ZERO) == 0) {
                result.add(divisor);
                a = divide(a, divisor);
            }
            divisor = divisor.add(BigInteger.TWO); // += 2
        }

        if (a.compareTo(BigInteger.ONE) > 0)
            result.add(a);

        for (var d : result) {
            if (d.bitLength() > n)
                throw new IllegalArgumentException(String.format("Number %s is undecomposed in GF(2^%s)", initialA, n));
        }

        return result;
    }

    private BigInteger divide(BigInteger dividend, BigInteger divisor) {
        return divMod(dividend, divisor, true);
    }

    private BigInteger remainder(BigInteger dividend, BigInteger divisor) {
        return divMod(dividend, divisor, false);
    }

    private BigInteger divMod(BigInteger dividend, BigInteger divisor, boolean isDivide) {
        var bitLengthDividend = dividend.bitLength();
        var bitLengthDivisor = divisor.bitLength();
        var result = BigInteger.ZERO;

        var start = bitLengthDividend - 1;
        var end = bitLengthDivisor - 1;
        for (var i = start; i >= end; --i) {
            var shiftedNumber = dividend.shiftRight(i);
            var firstBit = shiftedNumber.and(BigInteger.ONE); // (dividend >> i) & 1
            if (firstBit.compareTo(BigInteger.ZERO) == 0) // == 0
                continue;

            var shift = i - bitLengthDivisor + 1;
            var shiftedDivisor = divisor.shiftLeft(shift); // divisor << (i - bitLengthDivisor + 1);
            dividend = dividend.xor(shiftedDivisor);

            var current = BigInteger.ONE.shiftLeft(shift);
            result = result.or(current);
        }
        return isDivide ? result : dividend;
    }

    private byte remainder(int dividend, byte divisor) {
        var bitLengthValue = getBitLength(dividend);
        var bitLengthMod = getBitLength(divisor);
        for (int i = bitLengthValue - 1; i >= bitLengthMod - 1; --i) {
            if (((dividend >> i) & 1) == 0)
                continue;

            dividend ^= (divisor & 0xFF) << (i - bitLengthMod + 1);
        }

        return (byte) dividend;
    }

    private byte reduce8Degree(int value, byte mod) {
        var bitLengthValue = getBitLength(value);
        var bitLengthMod = 9;
        for (int i = bitLengthValue - 1; i >= bitLengthMod - 1; --i) {
            if (((value >> i) & 1) == 0)
                continue;

            value ^= ((mod & 0xFF) | 0b1_0000_0000) << (i - bitLengthMod + 1);
        }

        return (byte) value;
    }

    private int getBitLength(byte a) {
        return getBitLength(a & 0xFF);
    }

    private int getBitLength(int a) {
        var bitLength = 0;
        while (a > 0) {
            ++bitLength;
            a >>= 1;
        }
        return bitLength;
    }
}

