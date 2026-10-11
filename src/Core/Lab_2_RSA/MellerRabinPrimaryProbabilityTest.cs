using System.Numerics;

namespace Core.Lab_2_RSA;

public class MellerRabinPrimaryProbabilityTest : BasePrimaryProbabilityTest
{
    private struct NumberDecomposition(BigInteger d, BigInteger s)
    {
        // n - 1 = 2^d * s
        public BigInteger D = d;
        public BigInteger S = s;
    }

    protected override double ErrorProbability { get; } = 0.25;

    protected override bool PrimaryProbabilityTestStep(BigInteger n, BigInteger a)
    {
        if (Math.EuclidAlgorithm(a, n) != 1)
            return false;

        var numberDecomposition = DecomposeNumber(n - 1);

        var pow = Math.FastBinaryPowModule(a, numberDecomposition.S, n);
        if (pow == 1 || pow == n - 1)
            return true;

        for (var i = BigInteger.One; i < numberDecomposition.D; ++i)
        {
            pow = (pow * pow) % n;
            if (pow == 1)
                return false;
            if (pow == n - 1)
                return true;
        }

        return false;
    }

    private NumberDecomposition DecomposeNumber(BigInteger number)
    {
        BigInteger count = 0;
        while (number % 2 == 0)
        {
            ++count;
            number >>= 1;
        }

        return new NumberDecomposition(count, number);
    }
}