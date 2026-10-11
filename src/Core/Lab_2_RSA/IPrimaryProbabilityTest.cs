using System.Numerics;

namespace Core.Lab_2_RSA;

public interface IPrimaryProbabilityTest
{
    public bool PrimaryProbabilityTest(BigInteger number, double probability);
}