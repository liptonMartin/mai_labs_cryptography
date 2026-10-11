using System.Numerics;

namespace Core.Lab_2_RSA;

public class SolovayStrassenPrimaryProbabilityTest : BasePrimaryProbabilityTest
{
    protected override bool PrimaryProbabilityTestStep(BigInteger n, BigInteger a)
    {
        if (Math.EuclidAlgorithm(n, a) != 1)
            return false;

        var jacobi = Math.CalcJacobiSymbol(a, n) % n;
        if (jacobi < 0)
            jacobi += n;

        var pow = Math.FastBinaryPowModule(a, (n - 1) / 2, n);
        return jacobi == pow;
    }

    protected override double ErrorProbability { get; } = 0.5;
}