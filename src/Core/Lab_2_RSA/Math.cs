using System.Numerics;

namespace Core.Lab_2_RSA;

public static class Math
{
    public struct BezoutEquationCoefficients(BigInteger x, BigInteger y, BigInteger gcd)
    {
        public readonly BigInteger X = x;
        public readonly BigInteger Y = y;
        public readonly BigInteger Gcd = gcd;
    }

    public readonly struct NumberPower(BigInteger number, BigInteger power)
    {
        public readonly BigInteger Number = number;
        public readonly BigInteger Power = power;
    }

    public static BigInteger CalcFunctionCarmichaelByFactorization(BigInteger number)
    {
        var factorization = CalcFactorization(number);
        BigInteger result = 1;
        foreach (var numberPower in factorization)
        {
            if (numberPower.Number % 2 != 0)
            {
                var step = FastBinaryPow(numberPower.Number, numberPower.Power - 1) * (numberPower.Number - 1);
                result = CalcLcm(result, step);
            }
            else if (numberPower.Power == 2)
                result = CalcLcm(result, 2);
            else if (numberPower.Power >= 3)
                result = CalcLcm(result, FastBinaryPow(2, numberPower.Power - 2));
        }

        return result;
    }

    public static BigInteger CalcFunctionEulerByDpf(BigInteger number)
    {
        double result = 0;
        for (BigInteger k = 1; k <= number; ++k)
        {
            var gcd = EuclidAlgorithm(number, k);
            var angle = (2 * System.Math.PI * (double)k) / (double)number;
            var cos = System.Math.Cos(angle);
            result += (double)gcd * cos;
        }

        return new BigInteger(System.Math.Round(result));
    }

    public static BigInteger CalcFunctionEulerByFactorization(BigInteger number)
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

    public static BigInteger CalcFunctionEulerSumUnit(BigInteger a)
    {
        BigInteger result = 0;
        for (var i = 1; i < a; ++i)
            if (EuclidAlgorithm(a, i) == 1)
                result += 1;

        return result;
    }

    public static BigInteger BinaryEuclidAlgorithm(BigInteger a, BigInteger b)
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

    public static BezoutEquationCoefficients ExtendedEuclidAlgorithm(BigInteger a, BigInteger b)
    {
        if (b == 0)
            return new BezoutEquationCoefficients(1, 0, a);

        var coefficients = ExtendedEuclidAlgorithm(b, a % b);
        var x1 = coefficients.Y;
        var y1 = coefficients.X - (a / b) * coefficients.Y;
        return new BezoutEquationCoefficients(x1, y1, coefficients.Gcd);
    }

    public static BigInteger EuclidAlgorithm(BigInteger a, BigInteger b)
    {
        while (a != 0 && b != 0)
            (a, b) = (b, a % b);
        return a + b;
    }

    public static BigInteger CalcJacobiSymbol(BigInteger a, BigInteger n)
    {
        BigInteger result = 1;
        a %= n;
        while (a != 0)
        {
            while (a % 2 == 0)
            {
                result *= n % 8 == 1 || n % 8 == 7 ? 1 : -1;
                a /= 2;
            }

            if (a % 4 == 3 && n % 4 == 3)
                result *= -1;

            (a, n) = (n, a);
            a %= n;
        }

        return n == 1 ? result : 0;
    }

    public static BigInteger CalcLegendreSymbol(BigInteger a, BigInteger p)
    {
        var result = FastBinaryPowModule(a, (p - 1) / 2, p);
        return result == p - 1 ? -1 : (int)result;
    }

    public static BigInteger FastBinaryPowModule(BigInteger a, BigInteger b, BigInteger mod)
    {
        BigInteger accumulator = a;
        BigInteger result = 1;
        while (b != 0)
        {
            if (b % 2 == 1)
                result = (result * accumulator) % mod;
            accumulator = (accumulator * accumulator) % mod;
            b >>= 1;
        }

        return result % mod;
    }

    public static BigInteger FastBinaryPow(BigInteger a, BigInteger b)
    {
        BigInteger accumulator = a;
        BigInteger result = 1;
        while (b != 0)
        {
            if (b % 2 == 1)
                result *= accumulator;
            accumulator *= accumulator;
            b >>= 1;
        }

        return result;
    }

    public static List<NumberPower> CalcFactorization(BigInteger number)
    {
        var result = new List<NumberPower>();
        for (var divisor = new BigInteger(2); divisor * divisor <= number; ++divisor)
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

    public static BigInteger CalcLcm(BigInteger a, BigInteger b)
    {
        return (a * b) / EuclidAlgorithm(a, b);
    }
}