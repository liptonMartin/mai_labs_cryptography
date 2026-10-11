namespace Core.Lab_2_RSA;

public static class Math
{
    public struct BezoutEquationCoefficients(long x, long y, uint gcd)
    {
        public readonly long X = x;
        public readonly long Y = y;
        public readonly uint Gcd = gcd;
    }

    public readonly struct NumberPower(uint number, uint power)
    {
        public readonly uint Number = number;
        public readonly uint Power = power;
    }

    public static ulong CalcFunctionCarmichaelByFactorization(uint number)
    {
        var factorization = CalcFactorization(number);
        ulong result = 1u;
        foreach (var numberPower in factorization)
        {
            if (numberPower.Number % 2 != 0)
            {
                var step = FastBinaryPow(numberPower.Number, numberPower.Power - 1) * (numberPower.Number - 1);
                result = CalcLcm(result, step);
                continue;
            }

            switch (numberPower.Power)
            {
                case 1:
                    break;
                case 2:
                    result = CalcLcm(result, 2);
                    break;
                default:
                    var step = FastBinaryPow(2, numberPower.Power - 2);
                    result = CalcLcm(result, step);
                    break;
            }
        }

        return result;
    }

    public static double CalcFunctionEulerByDpf(uint number)
    {
        double result = 0;
        for (var k = 1u; k <= number; ++k)
        {
            var gcd = EuclidAlgorithm(number, k);
            var angle = (2 * System.Math.PI * k) / number;
            var cos = System.Math.Cos(angle);
            result += gcd * cos;
        }

        return result;
    }

    public static uint CalcFunctionEulerByFactorization(uint number)
    {
        var factorization = CalcFactorization(number);
        var result = number;
        foreach (var numberPower in factorization)
        {
            result /= numberPower.Number;
            result *= numberPower.Number - 1;
        }

        return result;
    }

    public static uint CalcFunctionEulerSumUnit(uint a)
    {
        var result = 0u;
        for (var i = 1u; i < a; ++i)
        {
            if (EuclidAlgorithm(a, i) == 1)
                result += 1;
        }

        return result;
    }

    public static uint BinaryEuclidAlgorithm(uint a, uint b)
    {
        if (a == 0)
            return b;
        if (b == 0)
            return a;

        var count = 0;
        while ((a & 1u) == 0 && (b & 1u) == 0)
        {
            ++count;
            a >>= 1;
            b >>= 1;
        }

        while ((a & 1u) == 0)
            a >>= 1;

        while (b != 0)
        {
            while ((b & 1u) == 0)
                b >>= 1;

            if (a > b)
                (a, b) = (b, a);

            b -= a;
        }

        return a << count;
    }

    public static BezoutEquationCoefficients ExtendedEuclidAlgorithm(uint a, uint b)
    {
        if (b == 0)
            return new BezoutEquationCoefficients(1, 0, a);

        var coefficients = ExtendedEuclidAlgorithm(b, a % b);
        var x1 = coefficients.Y;
        var y1 = coefficients.X - (a / b) * coefficients.Y;
        return new BezoutEquationCoefficients(x1, y1, coefficients.Gcd);
    }

    public static uint EuclidAlgorithm(uint a, uint b)
    {
        if (b > a)
            (a, b) = (b, a);
        while (a != 0 && b != 0)
            (a, b) = (b, a % b);
        return a + b;
    }

    public static ulong EuclidAlgorithm(ulong a, ulong b)
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

    public static ulong FastBinaryPow(uint a, uint b)
    {
        ulong accumulator = a;
        ulong result = 1;
        while (b != 0)
        {
            if (b % 2 == 1)
                result *= accumulator;
            accumulator *= accumulator;
            b >>= 1;
        }

        return result;
    }

    public static List<NumberPower> CalcFactorization(uint number)
    {
        var result = new List<NumberPower>();
        for (var divisor = 2u; (ulong)divisor * divisor <= number; ++divisor)
        {
            uint count = 0;
            while (number % divisor == 0)
            {
                ++count;
                number /= divisor;
            }

            if (count != 0)
                result.Add(new NumberPower(divisor, count));
        }

        if (number > 1)
            result.Add(new NumberPower(number, 1));

        return result;
    }

    public static ulong CalcLcm(uint a, uint b)
    {
        return ((ulong)a * b) / EuclidAlgorithm(a, b);
    }

    public static ulong CalcLcm(ulong a, ulong b)
    {
        return (a * b) / EuclidAlgorithm(a, b);
    }
}