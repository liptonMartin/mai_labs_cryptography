using System.Numerics;

namespace Core.Lab_2_RSA;

public class FermatPrimaryProbabilityTest : BasePrimaryProbabilityTest
{
    protected override bool PrimaryProbabilityTestStep(BigInteger n, BigInteger a)
    {
        if (Math.BinaryEuclidAlgorithm(n, a) != 1)
            return false;

        return Math.FastBinaryPowModule(a, n - 1, n) == 1;
    }

    protected override double ErrorProbability { get; } = 0.5;
}