using System.Numerics;
using Core.Lab_2_RSA;

namespace Tests.TestsLab2Rsa;

[TestFixture]
public class TestContinuedFraction
{
    [Test]
    public void BaseTestDecomposeFraction()
    {
        var continuedFraction = new ContinuedFraction(67, 13);
        var decomposedFraction = continuedFraction.DecomposeFractionToContinuedFraction();

        var expected = new List<BigInteger> { 5, 6, 2 };
        Assert.That(decomposedFraction, Is.EqualTo(expected));
    }

    [Test]
    public void BaseTestGetConvergentFractions()
    {
        var continuedFraction = new ContinuedFraction(67, 13);

        var convergentFractions = continuedFraction.GetConvergentFractions();
        var expected = new List<Fraction>
        {
            new(1, 0),
            new(5, 1),
            new(31, 6),
            new(67, 13)
        };

        Assert.That(convergentFractions, Is.EqualTo(expected));
    }
}