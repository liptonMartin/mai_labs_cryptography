namespace Core.Lab_2_RSA;

public static class Math
{
    public static uint EuclidAlgorithm(uint a, uint b)
    {
        if (b > a)
            (a, b) = (b, a);
        while (a != 0 && b != 0)
            (a, b) = (b, a % b);
        return a + b;
    }
    
    public static int CalcJacobiSymbol(uint a, uint n)
    {
        var result = 1;
        a %= n;
        while (a != 0)
        {
            while (a % 2 == 0)
            {
                result *= n % 8 is 1 or 7 ? 1 : -1;
                a /= 2;
            }

            if (a % 4 == 3 && n % 4 == 3)
                result *= -1;

            (a, n) = (n, a);
            a %= n;
        }

        return n == 1 ? result : 0;
    }
    
    public static int CalcLegendreSymbol(uint a, uint p)
    {
        var result = FastBinaryPowModule(a, (p - 1) / 2, p);
        return result == p - 1 ? -1 : (int)result;
    }
    
    public static uint FastBinaryPowModule(uint a, uint b, uint mod)
    {
        ulong accumulator = a;
        ulong result = 1;
        while (b != 0)
        {
            if (b % 2 == 1)
                result = (result * accumulator) % mod;
            accumulator = (accumulator * accumulator) % mod;
            b >>= 1;
        }

        return (uint)result % mod;
    }
}