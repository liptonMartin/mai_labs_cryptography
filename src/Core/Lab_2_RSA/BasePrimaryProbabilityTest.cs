using System.Numerics;

namespace Core.Lab_2_RSA;

public abstract class BasePrimaryProbabilityTest : IPrimaryProbabilityTest
{
    protected abstract double ErrorProbability { get; }

    public bool PrimaryProbabilityTest(BigInteger number, uint probability)
    {
        var countIterations = System.Math.Ceiling(System.Math.Log(probability, ErrorProbability));
        var random = new Random();
        for (var it = 0; it < countIterations; ++it)
        {
            var a = RandomBigInteger(random, 124);
            var result = PrimaryProbabilityTestStep(number, a);
            if (!result)
                return false;
        }

        return true;
    }

    protected abstract bool PrimaryProbabilityTestStep(BigInteger n, BigInteger a);

    private BigInteger RandomBigInteger(Random random, uint length)
    {
        var bytes = new byte[length];
        random.NextBytes(bytes);
        return new BigInteger(bytes, isUnsigned: true);
    }
}