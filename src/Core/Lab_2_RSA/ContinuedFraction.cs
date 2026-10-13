using System.Numerics;

namespace Core.Lab_2_RSA;

public class ContinuedFraction(BigInteger numerator, BigInteger denominator) : Fraction(numerator, denominator)
{
    public List<BigInteger> DecomposeFractionToContinuedFraction()
    {
        var numerator = Numerator;
        var denominator = Denominator;

        var result = new List<BigInteger>();
        while (denominator != 0)
        {
            result.Add(numerator / denominator);

            var remainder = numerator % denominator;
            numerator = denominator;
            denominator = remainder;
        }

        return result;
    }

    public List<Fraction> GetConvergentFractions()
    {
        var decomposedContinuedFraction = DecomposeFractionToContinuedFraction();
        var result = new List<Fraction> { new(1, 0), new(decomposedContinuedFraction[0], 1) };

        var n = decomposedContinuedFraction.Count;
        for (var i = 2; i <= n; ++i)
        {
            var numerator = result[^1].Numerator * decomposedContinuedFraction[i - 1] + result[^2].Numerator;
            var denominator = result[^1].Denominator * decomposedContinuedFraction[i - 1] + result[^2].Denominator;

            result.Add(new Fraction(numerator, denominator));
        }

        return result;
    }
}