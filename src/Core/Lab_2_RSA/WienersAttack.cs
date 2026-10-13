using System.Numerics;

namespace Core.Lab_2_RSA;

public class WienersAttack(KeyGenerator.Key publicKey)
{
    public KeyGenerator.Key PublicKey { get; } = publicKey;

    public BigInteger Hack()
    {
        var continuedFraction = new ContinuedFraction(PublicKey.Exp, PublicKey.N);
        var convergentFractions = continuedFraction.GetConvergentFractions();

        foreach (var convergentFraction in convergentFractions)
        {
            var k = convergentFraction.Numerator;
            var d = convergentFraction.Denominator;
            if (k == 0)
                continue;
            if (PublicKey.Exp * d % k != 1)
                continue;

            var N = PublicKey.N;
            var phiN = (PublicKey.Exp * d - 1) / k;

            var b = N - phiN + 1;
            var c = N;

            var discriminant = Math.FastBinaryPow(b, 2) - 4 * c;
            var floorSqrt = Math.FloorSqrt(discriminant);

            if (floorSqrt * floorSqrt == discriminant)
            {
                var p = (b - floorSqrt) / 2;
                var q = (b + floorSqrt) / 2;

                return p * q;
            }
        }

        throw new InvalidOperationException("Failed to hack!");
    }
}