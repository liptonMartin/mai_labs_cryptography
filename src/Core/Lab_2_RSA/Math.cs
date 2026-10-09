namespace Core.Lab_2_RSA;

public static class Math
{
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