using System.Numerics;

namespace Core.Lab_2_RSA;

public abstract class BasePrimaryProbabilityTest : IPrimaryProbabilityTest
{
    protected abstract double ErrorProbability { get; }

    public bool PrimaryProbabilityTest(BigInteger number, double probability)
    {
        if (probability is >= 1 or < 0.5)
            throw new InvalidOperationException($"Invalid argument probability = {probability}");

        if (number == 2)
            return true;
        if (number == 1)
            return false;

        var countIterations = System.Math.Ceiling(System.Math.Log(1 - probability, ErrorProbability));
        var random = new Random();
        for (var it = 0; it < countIterations; ++it)
        {
            var a = (RandomBigInteger(random, number.GetByteCount()) % (number - 1)) + 1;
            var result = PrimaryProbabilityTestStep(number, a);
            if (!result)
                return false;
        }

        return true;
    }

    protected abstract bool PrimaryProbabilityTestStep(BigInteger n, BigInteger a);

    private BigInteger RandomBigInteger(Random random, int length)
    {
        var bytes = new byte[length];
        random.NextBytes(bytes);
        return new BigInteger(bytes, isUnsigned: true);
    }
}