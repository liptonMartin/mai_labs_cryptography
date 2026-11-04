package common;

public class Helper {
    public static int binaryPow(int a, int n, int mod) {
        if (n == 0)
            return 1 % mod;

        long result = 1;
        long base = a;
        while (n > 0) {
            if ((n & 1) == 1)
                result = (result * base) % mod;

            base = (base * base) % mod;
            n >>= 1;
        }

        return (int) result;
    }
}
