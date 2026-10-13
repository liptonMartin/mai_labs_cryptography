using System.Numerics;

namespace Core.Lab_2_RSA;

public class Fraction(BigInteger numerator, BigInteger denominator)
{
    public BigInteger Numerator { get; } = numerator;
    public BigInteger Denominator { get; } = denominator;

    private bool Equals(Fraction? other) =>
        other is not null &&
        Numerator == other.Numerator &&
        Denominator == other.Denominator;

    public override bool Equals(object? obj) => Equals(obj as Fraction);

    public override int GetHashCode() => HashCode.Combine(Numerator, Denominator);
}