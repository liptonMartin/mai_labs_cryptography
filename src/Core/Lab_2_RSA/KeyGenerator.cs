using System.ComponentModel;
using System.Numerics;

namespace Core.Lab_2_RSA;

public class KeyGenerator(PrimaryTestType primaryTestType, double probability, uint bitLength)
{
    public PrimaryTestType TestType { get; set; } = primaryTestType;

    public double Probability
    {
        get;
        set
        {
            if (value is >= 1 or < 0.5)
                throw new ArgumentException($"Probability should in [0,5, 1). Current value is {value}");
            field = value;
        }
    } = probability;

    public uint BitLength
    {
        get;
        set
        {
            if (value % 8 != 0)
                throw new ArgumentException($"Bit length should be divided to 8! Current value is {value}");
            field = value;
        }
    } = bitLength;

    public struct Key(BigInteger exp, BigInteger n)
    {
        public BigInteger Exp = exp;
        public BigInteger N = n;
    }

    public struct RsaKeys(BigInteger e, BigInteger d, BigInteger n)
    {
        public Key PublicKey = new(e, n);
        public Key PrivateKey = new(d, n);
    }

    public RsaKeys GenerateKeys()
    {
        var random = new Random();
        var byteLength = BitLength / 8;
        BigInteger p, q;
        do
        {
            var bytes1 = new byte[byteLength];
            var bytes2 = new byte[byteLength];
            random.NextBytes(bytes1);
            random.NextBytes(bytes2);

            p = new BigInteger(bytes1, isUnsigned: true);
            q = new BigInteger(bytes2, isUnsigned: true);
        } while (!IsCorrectPQ(p, q) || !IsPrimeProbably(p) || !IsPrimeProbably(q));

        var n = p * q;

        var phiN = (p - 1) * (q - 1);
        var e = phiN - 10;
        for (; e > 1; --e)
            if (Math.EuclidAlgorithm(e, phiN) == 1)
                break;

        if (Math.EuclidAlgorithm(e, phiN) != 1)
            throw new InvalidOperationException("Internal error: failed to search encryption exponent");

        var d = CalcDecryptExponent(e, phiN);
        if (e * d % phiN != 1)
            throw new InvalidOperationException("Internal error: failed to calc decryption exponent");

        return new RsaKeys(e, d, n);
    }

    private BigInteger CalcDecryptExponent(BigInteger e, BigInteger phiN)
    {
        var result = Math.ExtendedEuclidAlgorithm(e, phiN);
        return result.X > 0 ? result.X : result.X + phiN;
    }

    private bool IsPrimeProbably(BigInteger number)
    {
        BasePrimaryProbabilityTest test = TestType switch
        {
            PrimaryTestType.Fermat => new FermatPrimaryProbabilityTest(),
            PrimaryTestType.SolovayStrassen => new SolovayStrassenPrimaryProbabilityTest(),
            PrimaryTestType.MellerRabin => new MellerRabinPrimaryProbabilityTest(),
            _ => throw new InvalidEnumArgumentException($"Unknown value in enum {nameof(PrimaryTestType)}!")
        };
        return test.PrimaryProbabilityTest(number, Probability);
    }

    private bool IsCorrectPQ(BigInteger p, BigInteger q)
    {
        return p != 0 && q != 0 && p != q;
    }
}